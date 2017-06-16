using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Domain
{
    public delegate void SetHandler<T>(T source, object value);

    class MemberCacheKey
    {
        public MemberCacheKey(Type modelType, string memberName)
        {
            ModelType = modelType;
            MemberName = memberName;
        }

        Type ModelType { get; }
        string MemberName { get; }

        protected bool Equals(MemberCacheKey other)
        {
            return Equals(ModelType, other.ModelType) && string.Equals(MemberName, other.MemberName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemberCacheKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModelType != null ? ModelType.GetHashCode() : 0) * 397) ^ (MemberName != null ? MemberName.GetHashCode() : 0);
            }
        }
    }

    public class ModelCloner<T> where T : BaseModel
    {
        internal static readonly Hashtable ValueTypesLoaders = new Hashtable
        {
            [typeof(sbyte)] = OpCodes.Ldind_I1,
            [typeof(byte)] = OpCodes.Ldind_U1,
            [typeof(char)] = OpCodes.Ldind_U2,
            [typeof(short)] = OpCodes.Ldind_I2,
            [typeof(ushort)] = OpCodes.Ldind_U2,
            [typeof(int)] = OpCodes.Ldind_I4,
            [typeof(uint)] = OpCodes.Ldind_U4,
            [typeof(long)] = OpCodes.Ldind_I8,
            [typeof(ulong)] = OpCodes.Ldind_I8,
            [typeof(bool)] = OpCodes.Ldind_I1,
            [typeof(double)] = OpCodes.Ldind_R8,
            [typeof(float)] = OpCodes.Ldind_R4
        };
        static readonly ConcurrentDictionary<MemberCacheKey, object> settersCache = new ConcurrentDictionary<MemberCacheKey, object>();

        private readonly BaseModel _original;
        readonly List<Tuple<SetHandler<T>, object>> _assignements = new List<Tuple<SetHandler<T>, object>>();

        public ModelCloner(BaseModel original)
        {
            _original = original;
        }

        string GetMemberName<TP>(Expression<Func<T, TP>> member)
        {
            var memberExpression = member.Body as MemberExpression;
            var propertyInfo = memberExpression?.Member as PropertyInfo;
            if (propertyInfo != null)
            {
                return propertyInfo.Name;
            }

            throw new InvalidOperationException("Cannot resolve member name");
        }

        public ModelCloner<T> With<TP>(Expression<Func<T, TP>> member, TP value)
        {
            var memberName = GetMemberName(member);

            SetHandler<T> setter;
            var cacheKey = new MemberCacheKey(typeof(T), memberName);
            if (settersCache.ContainsKey(cacheKey))
            {
                setter = (SetHandler<T>) settersCache[cacheKey];
            }
            else
            {
                setter = BuildSetter(member);
                settersCache.AddOrUpdate(cacheKey, setter, (_, __) => setter);
            }

            _assignements.Add(new Tuple<SetHandler<T>, object>(setter, value));

            return this;
        }

        public T Clone()
        {
            var copy = (T) _original.Clone();
            foreach (var assignement in _assignements)
            {
                var setter = assignement.Item1;
                var value = assignement.Item2;
                setter(copy, value);
            }

            return copy;
        }

        internal static SetHandler<T> BuildSetter<TP>(Expression<Func<T, TP>> member)
        {
            SetHandler<T> setter = null;

            var memberExpression = member.Body as MemberExpression;
            var propertyInfo = memberExpression?.Member as PropertyInfo;
            if (propertyInfo != null)
            {
                var name = propertyInfo.Name;
                var field = typeof(T).GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

                var type = typeof(T);
                var dm = new DynamicMethod("setter", typeof(void), new[] { type, typeof(object) }, type, true);
                var setGenerator = dm.GetILGenerator();

                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);

                if (field.FieldType.IsValueType)
                {
                    setGenerator.Emit(OpCodes.Unbox, field.FieldType); //Unbox it 
                    if (ValueTypesLoaders[field.FieldType] != null) //and load
                    {
                        var load = (OpCode)ValueTypesLoaders[field.FieldType];
                        setGenerator.Emit(load);
                    }
                    else
                    {
                        setGenerator.Emit(OpCodes.Ldobj, field.FieldType);
                    }
                }
                setGenerator.Emit(OpCodes.Stfld, field);
                setGenerator.Emit(OpCodes.Ret);
                var delegateType = typeof(SetHandler<>).MakeGenericType(typeof(T));
                setter = (SetHandler<T>)dm.CreateDelegate(delegateType);
            }
            return setter;
        }
    }

    public abstract class BaseModel : ICloneable //<T> : ICloneable // BaseModel<T>
    {
        internal BaseModel()
        {
        }

        public object Clone() => MemberwiseClone();

        public ModelCloner<T> With<T, TP>(Expression<Func<T, TP>> member, TP value) where T : BaseModel
        {
            var cloner = new ModelCloner<T>(this);
            
            return cloner.With(member, value);
        }

        //[Obsolete]
        //public T With1<TP>(Expression<Func<T, TP>> member, TP value)
        //{
        //    var target = (T)Clone();
        //    var val = (object)value;

        //    var setter = BuildSetter(member);
        //    setter?.Invoke(target, val);

        //    return target;
        //}

        //internal static SetHandler<T> BuildSetter<TP>(Expression<Func<T, TP>> member)
        //{
        //    SetHandler<T> setter = null;

        //    var memberExpression = member.Body as MemberExpression;
        //    var propertyInfo = memberExpression?.Member as PropertyInfo;
        //    if (propertyInfo != null)
        //    {
        //        var name = propertyInfo.Name;
        //        var field = typeof(T).GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        //        var type = typeof(T);
        //        var dm = new DynamicMethod("setter", typeof(void), new[] { type, typeof(object) }, type, true);
        //        var setGenerator = dm.GetILGenerator();

        //        setGenerator.Emit(OpCodes.Ldarg_0);
        //        setGenerator.Emit(OpCodes.Ldarg_1);

        //        if (field.FieldType.IsValueType)
        //        {
        //            setGenerator.Emit(OpCodes.Unbox, field.FieldType); //Unbox it 
        //            if (ModelCloner<BaseModel<T>>.ValueTypesLoaders[field.FieldType] != null) //and load
        //            {
        //                var load = (OpCode)ModelCloner<BaseModel<T>>.ValueTypesLoaders[field.FieldType];
        //                setGenerator.Emit(load);
        //            }
        //            else
        //            {
        //                setGenerator.Emit(OpCodes.Ldobj, field.FieldType);
        //            }
        //        }
        //        setGenerator.Emit(OpCodes.Stfld, field);
        //        setGenerator.Emit(OpCodes.Ret);
        //        var delegateType = typeof(SetHandler<>).MakeGenericType(typeof(T));
        //        setter = (SetHandler<T>)dm.CreateDelegate(delegateType);
        //    }
        //    return setter;
        //}
    }
}