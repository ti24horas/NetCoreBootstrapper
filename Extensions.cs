namespace Bootstrapper
{
    using System;
    using System.Linq;
    using System.Reflection;

    [Obsolete("Should not be used anymore", true)]
    public static class Extensions
    {
        private static MethodInfo GetMethod(object o, string name, Type returnType, params Type[] types)
        {
            var methods = o.GetType().GetMethods().Where(a => a.Name == name).Where(a => a.ReturnType == (returnType ?? typeof(void)));
            foreach (var method in methods)
            {
                var parameters = method?.GetParameters();
                if (parameters?.Length == types.Length)
                {
                    if (types.Select((t, pos) => parameters[pos].ParameterType == t).All(s => s))
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        public static Action<T1, T2, T3> GetAction<T1, T2, T3>(object o, string methodName = "Configure")
        {
            var method = GetMethod(o, methodName, typeof(void), typeof(T1), typeof(T2), typeof(T3));
            return (t1, t2, t3) => method?.Invoke(o, new object[] { t1, t2, t3 });
        }

        public static Action<T1, T2> GetAction<T1, T2>(object o, string methodName = "Configure")
        {
            var method = GetMethod(o, methodName, typeof(void), typeof(T1), typeof(T2));
            return (t1, t2) => method?.Invoke(o, new object[] { t1, t2 });
        }

        public static Action<T1> GetAction<T1>(object o, string methodName = "Configure")
        {
            var method = GetMethod(o, methodName, typeof(void), typeof(T1));
            return t1 => method?.Invoke(o, new object[] { t1 });
        }

        public static Action GetAction(object o, string methodName = "Configure")
        {
            var method = GetMethod(o, methodName, typeof(void));
            return () => method?.Invoke(o, new object[0]);
        }

        public static Func<T1, T2, T3, TReturn> GetFunc<T1, T2, T3, TReturn>(object o, string methodName = "Configure")
        {
            var method = GetMethod(o, methodName, typeof(TReturn), typeof(T1), typeof(T2), typeof(T3));
            return (t1, t2, t3) => method == null ? default(TReturn) : (TReturn)method?.Invoke(o, new object[] { t1, t2, t3 });
        }

        public static Func<T1, T2, TReturn> GetFunc<T1, T2, TReturn>(object o, string methodName = "Configure")
        {
            var method = GetMethod(o, methodName, typeof(TReturn), typeof(T1), typeof(T2));
            return (t1, t2) => method == null ? default(TReturn) : (TReturn)method?.Invoke(o, new object[] { t1, t2 });
        }

        public static Func<T1, TReturn> GetFunc<T1, TReturn>(object o, string methodName = "Configure")
        {
            var method = GetMethod(o, methodName, typeof(TReturn), typeof(T1));
            return t1 => method == null ? default(TReturn) : (TReturn)method?.Invoke(o, new object[] { t1 });
        }
    }
}