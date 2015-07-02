using System;
using System.Collections.Generic;

namespace MediatR.Sagas
{
    /// <summary>
    ///     A hetrogenous container used for storing and retrieving strongly typed
    ///     instance values, keyed by their type
    /// </summary>
    public class HetrogenousContainer
    {
        #region Fields and Constructors

        private readonly IDictionary<Object, Object> map = new Dictionary<Object, Object>();

        #endregion

        #region Public Members

        /// <summary>
        ///     Attempts to add an instance into the container, that can later be retrieved by its type
        /// </summary>
        public HetrogenousContainer Put<T>(T value)
        {
            map.Add(ValueOfType<T>.Instance, value);
            return this;
        }

        /// <summary>
        ///     Attempts to get a typed instance from the container
        /// </summary>
        /// <typeparam name="T">The type which acts as a key for the stored instance</typeparam>
        /// <param name="value">The out value instance</param>
        /// <returns>Returns <c>true</c> if the instance was found</returns>
        public Boolean TryGet<T>(out T value)
        {
            Object objValue;

            if (map.TryGetValue(ValueOfType<T>.Instance, out objValue))
            {
                value = (T) objValue;
                return true;
            }

            value = default(T);
            return false;
        }

        #endregion

        #region Nested Types

        /// <summary>
        ///     A container class that substitutes for a key in the <see cref="HetrogenousContainer" />
        /// </summary>
        private sealed class ValueOfType<T>
        {
            #region Fields and Constructors

            private static readonly Lazy<ValueOfType<T>> lazyTypedValue =
                new Lazy<ValueOfType<T>>(() => new ValueOfType<T>());

            private ValueOfType()
            {
                Console.WriteLine("Creating a ValueOfType{{{0}}}", typeof(T));
            }

            #endregion

            #region Public Members

            /// <summary>
            ///     Gets a <see cref="ValueOfType{T}" /> instance for the given type
            /// </summary>
            public static ValueOfType<T> Instance
            {
                get { return lazyTypedValue.Value; }
            }

            #endregion
        }

        #endregion
    }
}