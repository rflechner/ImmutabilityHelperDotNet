using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace ImmutabilityHelperDotNet
{
    public class ModelCloner<T> where T : BaseModel<T>
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

        private readonly T _original;
        readonly List<Tuple<SetHandler<T>, object>> _assignements = new List<Tuple<SetHandler<T>, object>>();

        public ModelCloner(T original)
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
                var field = typeof(T)
                    .GetTypeInfo().GetDeclaredField($"<{name}>k__BackingField");

                var type = typeof(T);
                var dm = new DynamicMethod("setter", typeof(void), new[] { type, typeof(object) }, type, true);
                var setGenerator = dm.GetILGenerator();

                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);

                if (field.FieldType.GetTypeInfo().IsValueType)
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
}