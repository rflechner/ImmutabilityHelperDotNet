using System;
using System.Linq.Expressions;

namespace ImmutabilityHelperDotNet
{
    public abstract class BaseModel<T> where T : BaseModel<T>
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public ModelCloner<T> With<TP>(Expression<Func<T, TP>> member, TP value)
        {
            var cloner = new ModelCloner<T>((T)this);
            
            return cloner.With(member, value);
        }
    }
}
