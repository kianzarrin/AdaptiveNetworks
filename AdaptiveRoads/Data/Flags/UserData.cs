namespace AdaptiveRoads.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using KianCommons.Serialization;

    public static class IntFlagsExtensions {
        public static bool IsAnyFlagSet(this int a, int b) => (a & b) != 0;

        public static bool CheckFlags(this int value, int required, int forbidden) =>
            (value & (required | forbidden)) == required;

        public static int[] ToPow2Flags(this int flags) {
            List<int> ret = new();
            for (int i = 0; i < 32; ++i) {
                int flag = 1 << i;
                if (flags.IsAnyFlagSet(flag))
                    ret.Add(flags);
            }
            return ret.ToArray();
        }

        public static int[] ToFlagIndexes(this int flags) {
            List<int> ret = new();
            for (int i = 0; i < 32; ++i) {
                int flag = 1 << i;
                if (flags.IsAnyFlagSet(flag))
                    ret.Add(i);
            }
            return ret.ToArray();
        }

        public static T[] SetArraySizeOrCreate<T>(this T[] array, int? n, T def) {
            int size2 = n ?? 0;
            int size0 = array?.Length ?? 0;
            if(array != null && size0 == size2) {
                return array;
            }

            var ret = new T[size2];
            int size = Math.Min(size2, size0);
            for(int i= 0;i <size; ++i) {
                ret[i] = array[i];
            }
            for(int i = size; i< size2; ++i) {
                ret[i] = def;// initialization.
            }
            return ret;
        }

        public static T[] SetArraySize<T>(this T[] array, int? n, T def) {
            int size = n ?? 0;
            if (size == 0)
                return null; // don't allocate memory if not necessary
            else
                return array.SetArraySizeOrCreate(n, def);
        }
    }

    #region net instance data
    public struct UserData {
        public int[] UserValues;
        public int[] UserFlags;

        public void Serialize(SimpleDataSerializer s) {
            if (IsEmptyOrDefault()) {
                // if all values are default, then there is no need to store.
                s.WriteInt32Array(null);
                s.WriteInt32Array(null);
            } else {
                s.WriteInt32Array(UserValues);
                s.WriteInt32Array(UserFlags);
            }
        }
        public static UserData Deserialize(SimpleDataSerializer s) {
            return new UserData {
                UserValues = s.ReadInt32Array(),
                UserFlags = s.ReadInt32Array(),
            };
        }

        public void Allocate(UserDataNames ?names) {
            UserValues = UserValues.SetArraySize(names?.ValueNames?.Length  , 0);
            UserFlags = UserFlags.SetArraySize(names?.FlagsNames?.Length, 0);
        }

        public void RemoveValueAt(int i) {
            if(UserValues != null && UserValues.Length > i)
                UserValues = UserValues?.RemoveAt(i);
        }

        public void RemoveFlagAt(int i) {
            if (UserFlags != null && UserFlags.Length > i)
                UserFlags = UserFlags?.RemoveAt(i);
        }

        public bool IsEmptyOrDefault() {
            if (UserValues != null) {
                foreach (var userValue in UserValues) {
                    if (userValue != 0) {
                        return false;
                    }
                }
            }
            if (UserFlags != null) {
                foreach (var userFlags in UserFlags) {
                    if (userFlags != 0) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    #endregion

    #region model data
    public struct UserValue {
        public int Value;
        public bool Check(int value) => Value < 0 || value == Value;
    }

    [Serializable]
    public struct UserFlags {
        [BitMask]
        public int Required, Forbidden;
        public bool Check(int value) => value.CheckFlags(Required, Forbidden);
    }

    public class UserDataInfo {
        [CustomizableProperty("Selectors")]
        public UserValue[] UserValues;

        [CustomizableProperty("Flags")]
        public UserFlags[] UserFlags;

        public UserDataInfo() {
            UserValues = new UserValue[0];
            UserFlags = new UserFlags[0];
        }

        public void Allocate(UserDataNames names) {
            UserValues = UserValues.SetArraySizeOrCreate(names?.ValueNames?.Length, new UserValue { Value = -1 });
            UserFlags = UserFlags.SetArraySizeOrCreate(names?.FlagsNames?.Length, default(UserFlags));
        }

        public bool IsEmptyOrDefault() {
            if(UserValues != null) {
                foreach(var userValue in UserValues) {
                    if(userValue.Value != -1) {
                        return false;
                    }
                }
            }
            if (UserFlags != null) {
                foreach (var userFlags in UserFlags) {
                    if (userFlags.Required != 0 || userFlags.Forbidden != 0) {
                        return false;
                    }
                }
            }
            return true;
        }

        public void RemoveValueAt(int i) {
            if(UserValues != null && UserValues.Length > i) {
                UserValues = UserValues.RemoveAt(i);
            }
        }

        public void RemovFlagsAt(int i) {
            if (UserFlags != null && UserFlags.Length > i) {
                UserFlags = UserFlags.RemoveAt(i);
            }
        }

        public bool Check(UserData userData) {
            if(UserValues != null)
            {
                int n = Math.Min(UserValues.Length, userData.UserValues?.Length ?? 0);
                for (int i = 0; i < n; ++i) {
                    if (!UserValues[i].Check(userData.UserValues[i]))
                        return false;
                }
            }

            if(UserFlags != null) {
                int n = Math.Min(UserFlags.Length, userData.UserFlags?.Length ?? 0);
                for (int i = 0; i < n; ++i) {
                    if (!UserFlags[i].Check(userData.UserFlags[i]))
                        return false;
                }
            }

            return true;
        }
    }
    #endregion


    #region NetInfo data
    public struct UserValueNames {
        public string Title;
        public string[] Items;
    }

    public struct UserFlagsNames {
        public string Title;
        public string[] Items;
        public string [] ToNames(int flags) {
            List<string> ret = new();
            for (int i = 0; i < 32; ++i) {
                int flag = 1 << i;
                if (flags.IsAnyFlagSet(flag))
                    ret.Add(Items[i]);
            }
            return ret.ToArray();
        }
    }

    public class UserDataNames {
        public UserValueNames[] ValueNames;
        public UserFlagsNames[] FlagsNames;
        public void Add(UserValueNames entry) {
            ValueNames = ValueNames.AppendOrCreate(entry);
        }

        public void Add(UserFlagsNames entry) {
            FlagsNames.AppendOrCreate(entry);
        }

        public void RemoveValueAt(int i) {
            ValueNames = ValueNames?.RemoveAt(i);
        }
        public void RemoveFlagsAt(int i) {
            FlagsNames = FlagsNames?.RemoveAt(i);
        }

        public UserDataNames() {
            ValueNames = new UserValueNames[0];
            FlagsNames = new UserFlagsNames[0];
        }

        public UserDataNames Clone() => this.ShalowClone();

        public bool IsEmpty() {
            return ValueNames.IsNullorEmpty() && FlagsNames.IsNullorEmpty();
        }
    }

    public class UserDataNamesSet {
        public UserDataNames Node, Segment;
        public UserDataNamesSet() {
            Node = new();
            Segment = new();
        }
        public UserDataNamesSet Clone() {
            return new UserDataNamesSet {
                Node = Node.Clone(),
                Segment = Segment.Clone(),
            };
        }
    }
    #endregion
}
