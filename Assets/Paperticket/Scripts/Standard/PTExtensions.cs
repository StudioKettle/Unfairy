using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public static class PTExtensions {

        #region String


        /// <summary>
        /// Truncate the string to a max length
        /// </summary>
        /// <param name="self"></param>
        /// <param name="maxLength">The max number of characters the string can have</param>
        public static string Truncate(this string value, int maxLength) {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }


        /// <summary>
        /// Named format strings from object attributes. Eg: string blaStr = aPerson.ToString("My name is {FirstName} {LastName}.")
        /// </summary>
        /// <param name="self"></param>
        /// <param name="aFormat"></param>
        public static string ToString(this object anObject, string aFormat) {
            return ToString(anObject, aFormat, null);
        }

        /// <summary>
        /// Named format strings from object attributes. Eg: string blaStr = aPerson.ToString("My name is {FirstName} {LastName}.")
        /// </summary>
        /// <param name="anObject"></param>
        /// <param name="aFormat"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static string ToString(this object anObject, string aFormat, IFormatProvider formatProvider) {
            StringBuilder sb = new StringBuilder();
            Type type = anObject.GetType();
            Regex reg = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(aFormat);
            int startIndex = 0;
            foreach (Match m in mc) {
                Group g = m.Groups[2]; //it's second in the match between { and }
                int length = g.Index - startIndex - 1;
                sb.Append(aFormat.Substring(startIndex, length));

                string toGet = string.Empty;
                string toFormat = string.Empty;
                int formatIndex = g.Value.IndexOf(":"); //formatting would be to the right of a :
                if (formatIndex == -1) //no formatting, no worries
                {
                    toGet = g.Value;
                } else //pickup the formatting
                  {
                    toGet = g.Value.Substring(0, formatIndex);
                    toFormat = g.Value.Substring(formatIndex + 1);
                }

                //first try properties
                PropertyInfo retrievedProperty = type.GetProperty(toGet);
                Type retrievedType = null;
                object retrievedObject = null;
                if (retrievedProperty != null) {
                    retrievedType = retrievedProperty.PropertyType;
                    retrievedObject = retrievedProperty.GetValue(anObject, null);
                } else //try fields
                  {
                    FieldInfo retrievedField = type.GetField(toGet);
                    if (retrievedField != null) {
                        retrievedType = retrievedField.FieldType;
                        retrievedObject = retrievedField.GetValue(anObject);
                    }
                }

                if (retrievedType != null) //Cool, we found something
                {
                    string result = string.Empty;
                    if (toFormat == string.Empty) //no format info
                    {
                        result = retrievedType.InvokeMember("ToString",
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                            , null, retrievedObject, null) as string;
                    } else //format info
                      {
                        result = retrievedType.InvokeMember("ToString",
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                            , null, retrievedObject, new object[] { toFormat, formatProvider }) as string;
                    }
                    sb.Append(result);
                } else //didn't find a property with that name, so be gracious and put it back
                  {
                    sb.Append("{");
                    sb.Append(g.Value);
                    sb.Append("}");
                }
                startIndex = g.Index + g.Length + 1;
            }
            if (startIndex < aFormat.Length) //include the rest (end) of the string
            {
                sb.Append(aFormat.Substring(startIndex));
            }
            return sb.ToString();
        }

        #endregion



        #region Float / Int

        /// <summary>
        /// Remap the float into a new set of min/max values
        /// </summary>
        /// <param name="self"></param>
        /// <param name="valueRangeMin">The minimum of the current range </param>
        /// <param name="valueRangeMax">The maximum of the current range </param>
        /// <param name="newRangeMin">The minimum of the new range</param>
        /// <param name="newRangeMax">The maximum of the new range</param>
        public static float LinearRemap(this float value,
                                 float valueRangeMin, float valueRangeMax,
                                 float newRangeMin, float newRangeMax) {
            return (value - valueRangeMin) / (valueRangeMax - valueRangeMin) * (newRangeMax - newRangeMin) + newRangeMin;
        }

        /// <summary>
        /// Return the int with a random sign
        /// </summary>
        /// <param name="self"></param>
        /// <param name="negativeProbability">Optionally, set the probability that the result will be negative</param>
        public static int WithRandomSign(this int value, float negativeProbability = 0.5f) {
            return UnityEngine.Random.value < negativeProbability ? -value : value;
        }

        /// <summary>
        /// Return the float as a (1,1,1) vector 
        /// </summary>
        public static Vector3 ToVector(this float value) {
            return Vector3.one * value;
        }

        /// <summary>
        /// Return the float as the X value in a (1,0,0) vector 
        /// </summary>
        public static Vector3 ToXVector(this float value) {
            return Vector3.right * value;
        }

        /// <summary>
        /// Return the float as the Y value in a (0,1,0) vector 
        /// </summary>
        public static Vector3 ToYVector(this float value) {
            return Vector3.up * value;
        }

        /// <summary>
        /// Return the float as the Z value in a (0,0,1) vector 
        /// </summary>
        public static Vector3 ToZVector(this float value) {
            return Vector3.forward * value;
        }




        #endregion


        #region Layermask / Layer


        /// <summary>
        /// Test if layer is included in this layermask
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layer">The layer to test</param>
        public static bool Test (this LayerMask layerMask, int layer) {
            if (((1 << layer) & layerMask) != 0) return true;
            else return false;
        }

        #endregion


        #region GameObject

        /// <summary>
        /// Test if this gameobject is has the provided layer and tag
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layers">The layer the gameObject must be</param>
        /// <param name="tag">The tag the gameObject must be</param>
        /// <param name="debugging">Optionally print debug text to the console</param>
        public static bool CheckLayerAndTag(this GameObject gameObject, LayerMask layers, string tag, bool debugging = false) {
            if (!layers.Test(gameObject.layer)) {
                if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' layer '" + LayerMask.LayerToName(gameObject.layer) + "' is invalid, returning false.");
                return false;
            }
            if (tag.Length > 0) {
                string[] splitTags = tag.Split(',');
                foreach (string splitTag in splitTags) {
                    if (gameObject.tag == splitTag) {
                        if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
                        return true;
                    }
                }
            } else {
                if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
                return true;
            }
            if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' tag '" + gameObject.tag + "' is invalid, returning false.");
            return false;
            //if (tag.Contains(",")) {
            //    string[] splitTags = tag.Split(',');
            //    foreach (string splitTag in splitTags) {
            //        if (splitTag == tag) {
            //            if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
            //            return true;
            //        }
            //    }
            //    if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' tag '" + gameObject.tag + "' is invalid, returning false.");
            //    return false;
            //} else if (gameObject.tag != tag) {
            //    if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' tag '" + gameObject.tag + "' is invalid, returning false.");
            //    return false;
            //}                       
            //if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
            //return true;
        }

        /// <summary>
        /// Destroy this gameobject
        /// </summary>
        /// <param name="self"></param>
        public static void DestroyMe(this GameObject gameObject) {
            GameObject.Destroy(gameObject);
        }


        /// <summary>
        /// Check whether this gameObject contains the component
        /// </summary>
        public static bool HasComponent<T>(this GameObject gameObject) where T : MonoBehaviour {
            return gameObject.GetComponent<T>() != null;
        }



        /// <summary>
        /// Safely invoke a UnityEvent
        /// </summary>
        /// <typeparam name="T">The type of the UnityEvent</typeparam>
        /// <param name="self"></param>
        /// <param name="evt">The event</param>
        public static void Trigger(this GameObject self, UnityEvent evt) {
            if (evt != null) {
                evt.Invoke();
            } else {
                Debug.LogWarning("Tried to invoke a null UnityEvent");
            }
        }

        /// <summary>
        /// Safely invoke a UnityEvent
        /// </summary>
        /// <typeparam name="T">The type of the UnityEvent</typeparam>
        /// <param name="self"></param>
        /// <param name="evt">The event</param>
        /// <param name="data">The payload for the event</param>
        public static void Trigger<T>(this GameObject self, UnityEvent<T> evt, T data) {
            if (evt != null) {
                evt.Invoke(data);
            } else {
                Debug.LogWarning("Tried to invoke a null UnityEvent on " + self.name + " with type '" + typeof(T).ToString() + "' with the following payload: " + data.ToString());
            }
        }

        /// <summary>
        /// Safely invoke a UnityEvent2
        /// </summary>
        /// <typeparam name="T">The type of the UnityEvent2</typeparam>
        /// <param name="self"></param>
        /// <param name="evt">The event</param>
        public static void Trigger(this GameObject self, UnityEvent2 evt) {
            if (evt != null) {
                evt.Invoke();
            } else {
                Debug.LogWarning("Tried to invoke a null UnityEvent2");
            }
        }

        /// <summary>
        /// Safely invoke a UnityEvent2
        /// </summary>
        /// <typeparam name="T">The type of the UnityEvent2</typeparam>
        /// <param name="self"></param>
        /// <param name="evt">The event</param>
        /// <param name="data">The payload for the event</param>
        public static void Trigger<T>(this GameObject self, UnityEvent2<T> evt, T data) {
            if (evt != null) {
                evt.Invoke(data);
            } else {
                Debug.LogWarning("Tried to invoke a null UnityEvent2 on " + self.name + " with type '" + typeof(T).ToString() + "' with the following payload: " + data.ToString());
            }
        }




        /// <summary>
        /// Get a component, log an error if it's not there
        /// </summary>
        /// <typeparam name="T">The type of component to get</typeparam>
        /// <param name="self"></param>
        /// <returns>The component, if found</returns>
        public static T GetComponentRequired<T>(this GameObject self) where T : Component {
            T component = self.GetComponent<T>();

            if (component == null) Debug.LogError("Could not find " + typeof(T) + " on " + self.name);

            return component;
        }

        /// <summary>
        /// Perform an action if a component exists, skip otherwise
        /// </summary>
        /// <typeparam name="T">The type of component required</typeparam>
        /// <param name="self"></param>
        /// <param name="callback">The action to take</param>
        /// <returns>The component found</returns>
        public static T GetComponent<T>(this GameObject self, System.Action<T> callback) where T : Component {
            var component = self.GetComponent<T>();

            if (component != null) {
                callback.Invoke(component);
            }

            return component;
        }

        /// <summary>
        /// Take an action only if a component exists, error if it's not there
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="callback"></param>
        /// <returns>The component, if found</returns>
        public static T GetComponentRequired<T>(this GameObject self, System.Action<T> callback) where T : Component {
            var component = self.GetComponentRequired<T>();

            if (component != null) {
                callback.Invoke(component);
            }

            return component;
        }

        /// <summary>
        /// Get a component, take a different action if it isn't there
        /// </summary>
        /// <typeparam name="T">Component Type</typeparam>
        /// <param name="self">object being extended</param>
        /// <param name="success">Take this action if the component exists</param>
        /// <param name="failure">Take this action if the component does not exist</param>
        /// <returns></returns>
        public static T GetComponent<T>(this GameObject self, System.Action<T> success, System.Action failure) where T : Component {
            var component = self.GetComponent<T>();

            if (component != null) {
                success.Invoke(component);
                return component;
            } else {
                failure.Invoke();
                return null;
            }
        }

        #endregion




        #region Transform 


        /// <summary>
        /// Look at a GameObject
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look at</param>
        public static void LookAt(this Transform self, GameObject target) {
            self.LookAt(target.transform);
        }

        /// <summary>
        /// Find the rotation to look at a Vector3
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look at</param>
        /// <returns></returns>
        public static Quaternion GetLookAtRotation(this Transform self, Vector3 target) {
            return Quaternion.LookRotation(target - self.position);
        }

        /// <summary>
        /// Find the rotation to look at a Transform
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look at</param>
        /// <returns></returns>
        public static Quaternion GetLookAtRotation(this Transform self, Transform target) {
            return GetLookAtRotation(self, target.position);
        }

        /// <summary>
        /// Find the rotation to look at a GameObject
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look at</param>
        /// <returns></returns>
        public static Quaternion GetLookAtRotation(this Transform self, GameObject target) {
            return GetLookAtRotation(self, target.transform.position);
        }


        /// <summary>
        /// Instantly look away from a target Vector3
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look away from</param>
        public static void LookAwayFrom(this Transform self, Vector3 target) {
            self.rotation = GetLookAwayFromRotation(self, target);
        }

        /// <summary>
        /// Instantly look away from a target transform
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look away from</param>
        public static void LookAwayFrom(this Transform self, Transform target) {
            self.rotation = GetLookAwayFromRotation(self, target);
        }

        /// <summary>
        /// Instantly look away from a target GameObject
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look away from</param>
        public static void LookAwayFrom(this Transform self, GameObject target) {
            self.rotation = GetLookAwayFromRotation(self, target);
        }


        /// <summary>
        /// Find the rotation to look away from a target Vector3
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look away from</param>
        public static Quaternion GetLookAwayFromRotation(this Transform self, Vector3 target) {
            return Quaternion.LookRotation(self.position - target);
        }

        /// <summary>
        /// Find the rotation to look away from a target transform
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look away from</param>
        public static Quaternion GetLookAwayFromRotation(this Transform self, Transform target) {
            return GetLookAwayFromRotation(self, target.position);
        }

        /// <summary>
        /// Find the rotation to look away from a target GameObject
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">The thing to look away from</param>
        public static Quaternion GetLookAwayFromRotation(this Transform self, GameObject target) {
            return GetLookAwayFromRotation(self, target.transform.position);
        }

        /// <summary>
        /// Destroy all children of this transform
        /// </summary>
        /// <param name="self"></param>
        public static void DestroyChildren(this Transform transform) {
            for (var i = transform.childCount - 1; i >= 0; i--) {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Reset the transformation, setting it as its neutral pose
        /// </summary>
        /// <param name="self"></param>
        public static void ResetTransformation(this Transform transform) {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        #endregion



        #region Vector

        /// <summary>
        /// Return as a Vector2
        /// </summary>
        public static Vector2 xy(this Vector3 v) {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Return the vector with a new X value
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x">The new X value</param>
        public static Vector3 WithX(this Vector3 v, float x) {
            return new Vector3(x, v.y, v.z);
        }

        /// <summary>
        /// Return the vector with a new Y value
        /// </summary>
        /// <param name="self"></param>
        /// <param name="y">The new Y value</param>
        public static Vector3 WithY(this Vector3 v, float y) {
            return new Vector3(v.x, y, v.z);
        }

        /// <summary>
        /// Return the vector with a new Z value
        /// </summary>
        /// <param name="self"></param>
        /// <param name="z">The new Z value</param>
        public static Vector3 WithZ(this Vector3 v, float z) {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Return the vector with a new X value
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x">The new X value</param>
        public static Vector2 WithX(this Vector2 v, float x) {
            return new Vector2(x, v.y);
        }

        /// <summary>
        /// Return the vector with a new Y value
        /// </summary>
        /// <param name="self"></param>
        /// <param name="y">The new Y value</param>
        public static Vector2 WithY(this Vector2 v, float y) {
            return new Vector2(v.x, y);
        }

        /// <summary>
        /// Return the vector with a new Z value
        /// </summary>
        /// <param name="self"></param>
        /// <param name="z">The new Z value</param>
        public static Vector3 WithZ(this Vector2 v, float z) {
            return new Vector3(v.x, v.y, z);
        }


        /// <summary>
        /// Return the nearest point on this unit-vector in direction of an axis (i.e. a line that passes through zero)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="point">The point to find nearest on line for</param>
        /// <param name="isNormalized">Is the axis normalised?</param>
        public static Vector3 NearestPointOnAxis(this Vector3 axisDirection, Vector3 point, bool isNormalized = false) {
            if (!isNormalized) axisDirection.Normalize();
            var d = Vector3.Dot(point, axisDirection);
            return axisDirection * d;
        }

        /// <summary>
        /// As a unit vector pointing in either direction of a line, return the nearest point on that line to a given point
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pointOnLine">A point that the line passes through (defines an actual line in space)</param>
        /// <param name="point">The target point to find the nearest point on line for</param>
        /// <param name="isNormalized">Is this vector normalised?</param>
        public static Vector3 NearestPointOnInfiniteLine(
            this Vector3 lineDirection, Vector3 point, Vector3 pointOnLine, bool isNormalized = false) {
            if (!isNormalized) lineDirection.Normalize();
            var d = Vector3.Dot(point - pointOnLine, lineDirection);
            return pointOnLine + (lineDirection * d);
        }
        public static Vector3 NearestPointOnLine(this Vector3 pnt, Vector3 lineStart, Vector3 lineEnd) {
            var line = (lineEnd - lineStart);
            var len = line.magnitude;
            line.Normalize();
            var v = pnt - lineStart;
            var d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);
            return lineStart + line * d;
        }

        /// <summary>
        /// Return the closest vector from a list (e.g. A list of positions and know the closest of them)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="otherVectors">The list </param>
        /// <returns></returns>
        public static Vector2 GetClosestVector2From(this Vector2 vector, Vector2[] otherVectors) {
            if (otherVectors.Length == 0) throw new Exception("The list of other vectors is empty");
            var minDistance = Vector2.Distance(vector, otherVectors[0]);
            var minVector = otherVectors[0];
            for (var i = otherVectors.Length - 1; i > 0; i--) {
                var newDistance = Vector2.Distance(vector, otherVectors[i]);
                if (newDistance < minDistance) {
                    minDistance = newDistance;
                    minVector = otherVectors[i];
                }
            }
            return minVector;
        }

        /// <summary>
        /// Return the closest vector from a list (e.g. A list of positions and know the closest of them)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="otherVectors">The list </param>
        /// <returns></returns>
        public static Vector3 GetClosestVector3From(this Vector3 vector, Vector3[] otherVectors) {
            if (otherVectors.Length == 0) throw new Exception("The list of other vectors is empty");
            var minDistance = Vector3.Distance(vector, otherVectors[0]);
            var minVector = otherVectors[0];
            for (var i = otherVectors.Length - 1; i > 0; i--) {
                var newDistance = Vector3.Distance(vector, otherVectors[i]);
                if (newDistance < minDistance) {
                    minDistance = newDistance;
                    minVector = otherVectors[i];
                }
            }
            return minVector;
        }


        #endregion




        #region Rigidbodies

        /// <summary>
        /// Change the current direction of the rigidbody while keeping it’s velocity
        /// </summary>
        public static void ChangeDirection(this Rigidbody rb, Vector3 direction) {
            rb.velocity = direction.normalized * rb.velocity.magnitude;
        }


        /// <summary>
        /// Change the current direction of the rigidbody while keeping it’s velocity
        /// </summary>
        public static void ChangeDirection(this Rigidbody2D rb, Vector3 direction) {
            rb.velocity = direction.normalized * rb.velocity.magnitude;
        }


        /// <summary>
        /// Normalize the velocity of the rigidbody to a target speed while keeping the direction
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="magnitude">The target speed to set the rigidbody to</param>
        public static void NormalizeVelocity(this Rigidbody rb, float magnitude = 1) {
            rb.velocity = rb.velocity.normalized * magnitude;
        }

        #endregion



        #region UnityEvent2


        [Serializable]
        public class UnityEvent2Int : UnityEvent2<int> { }

        [Serializable]
        public class UnityEvent2Float : UnityEvent2<float> { }

        [Serializable]
        public class UnityEvent2String : UnityEvent2<string> { }

        [Serializable]
        public class UnityEvent2Bool : UnityEvent2<bool> { }

        [Serializable]
        public class UnityEvent2Vector3 : UnityEvent2<Vector3> { }

        [Serializable]
        public class UnityEvent2Vector2 : UnityEvent2<Vector2> { }

        [Serializable]
        public class UnityEvent2GameObject : UnityEvent2<GameObject> { }

        [Serializable]
        public class UnityEvent2Quaternion : UnityEvent2<Quaternion> { }

        [Serializable]
        public class UnityEvent2Transform : UnityEvent2<Transform> { }

        [Serializable]
        public class UnityEvent2Color : UnityEvent2<Color> { }

        [Serializable]
        public class UnityEvent2Texture2D : UnityEvent2<Texture2D> { }

        [Serializable]
        public class UnityEvent2AudioClip : UnityEvent2<AudioClip> { }

        #endregion




        #region Array / List


        /// <summary>
        /// For each component in an array, take an action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="callback">The action to take</param>
        public static void ForEachComponent<T>(this T[] array, System.Action<T> callback) where T : Component {
            for (var i = 0; i < array.Length; i++) {
                callback.Invoke(array[i]);
            }
        }

        /// <summary>
        /// For each component in an list, take an action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="callback">The action to take</param>
        public static void ForEachComponent<T>(this List<T> list, System.Action<T> callback) where T : Component {
            for (var i = 0; i < list.Count; i++) {
                callback.Invoke(list[i]);
            }
        }

        /// <summary>
        /// Shuffle the list in place using the Fisher-Yates method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list) {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Return a random item from the list.
        /// Sampling with replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static T RandomItem<T>(this IList<T> list) {
            if (list.Count == 0) throw new System.IndexOutOfRangeException("Cannot select a random item from an empty list");
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Removes a random item from the list, returning that item.
        /// Sampling without replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static T RemoveRandom<T>(this IList<T> list) {
            if (list.Count == 0) throw new System.IndexOutOfRangeException("Cannot remove a random item from an empty list");
            int index = UnityEngine.Random.Range(0, list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        #endregion

    }
}



#region Range


[Serializable]
public abstract class Range<T> {
    public T Min;
    public T Max;

    public abstract T RandomInRange();
}

[Serializable]
public class RangeInt : Range<int> {
    public RangeInt() {
        Min = 0;
        Max = 1;
    }
    public override int RandomInRange() {
        if (Min <= Max) return UnityEngine.Random.Range(Min, Max);
        return UnityEngine.Random.Range(Max, Min);
    }
}

[Serializable]
public class RangeFloat : Range<float> {
    public RangeFloat() {
        Min = 0;
        Max = 1;
    }

    public override float RandomInRange() {
        if (Min <= Max) return UnityEngine.Random.Range(Min, Max);
        return UnityEngine.Random.Range(Max, Min);
    }
}

[Serializable]
public class RangeColor : Range<Color> {
    public RangeColor() {
        Min = Color.HSVToRGB(0f, 0f, 0f);
        Max = Color.HSVToRGB(0.999f, 1f, 1f);
    }

    public override Color RandomInRange() {
        float minH, minS, minV, maxH, maxS, maxV;
        Color.RGBToHSV(Min, out minH, out minS, out minV);
        Color.RGBToHSV(Max, out maxH, out maxS, out maxV);

        return UnityEngine.Random.ColorHSV(minH, maxH, minS, maxS, minV, maxV);
    }
}

[Serializable]
public class RangeVector3 : Range<Vector3> {
    public RangeVector3() {
        Min = Vector3.zero;
        Max = Vector3.one;
    }

    public override Vector3 RandomInRange() {

        var final = Vector3.zero;

        if (Min.x <= Max.x) final.x = UnityEngine.Random.Range(Min.x, Max.x);
        else final.x = UnityEngine.Random.Range(Max.x, Min.x);

        if (Min.y <= Max.y) final.y = UnityEngine.Random.Range(Min.y, Max.y);
        else final.y = UnityEngine.Random.Range(Max.y, Min.y);

        if (Min.z <= Max.z) final.z = UnityEngine.Random.Range(Min.z, Max.z);
        else final.z = UnityEngine.Random.Range(Max.z, Min.z);

        return final;
    }
}


[Serializable]
public class RangeVector2 : Range<Vector2> {
    public RangeVector2() {
        Min = Vector2.zero;
        Max = Vector2.one;
    }

    public override Vector2 RandomInRange() {

        var final = Vector2.zero;

        if (Min.x <= Max.x) final.x = UnityEngine.Random.Range(Min.x, Max.x);
        else final.x = UnityEngine.Random.Range(Max.x, Min.x);

        if (Min.y <= Max.y) final.y = UnityEngine.Random.Range(Min.y, Max.y);
        else final.y = UnityEngine.Random.Range(Max.y, Min.y);

        return final;
    }
}


#endregion

