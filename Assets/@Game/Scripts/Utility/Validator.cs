using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectTA.Utility
{
    public static class Validator
    {
        /// <summary>
        /// Validates that an object is not null.
        /// </summary>
        /// <typeparam name="T">The type of the object to validate.</typeparam>
        /// <param name="obj">The object to validate.</param>
        /// <param name="errorMessage">The error message to log if validation fails.</param>
        /// <returns>True if the object is valid, false otherwise.</returns>
        public static bool ValidateNotNull<T>(T obj, string errorMessage) where T : class
        {
            if (obj == null)
            {
                Debug.LogError(errorMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates that a collection is not null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="errorMessage">The error message to log if validation fails.</param>
        /// <returns>True if the collection is valid, false otherwise.</returns>
        public static bool ValidateCollection<T>(IEnumerable<T> collection, string errorMessage)
        {
            if (collection == null || !collection.Any())
            {
                Debug.LogError(errorMessage);
                return false;
            }
            return true;
        }
    }
}