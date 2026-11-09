using System;
using System.Reflection;

namespace ProjectTA.Utility
{
    public static class TestUtility
    {
        public static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        public static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        public static T InvokePrivateMethod<T>(object obj, string methodName, object[] parameters)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

            // Get the private method info
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                throw new MissingMethodException($"The private method '{methodName}' was not found on type '{obj.GetType().Name}'.");
            }

            // Invoke the method and cast the result to T
            return (T)method.Invoke(obj, parameters);
        }

        public static void InvokePrivateMethod(object obj, string methodName, object[] parameters)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

            // Get the private method info
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                throw new MissingMethodException($"The private method '{methodName}' was not found on type '{obj.GetType().Name}'.");
            }

            // Invoke the method (no return type expected)
            method.Invoke(obj, parameters);
        }
    }
}