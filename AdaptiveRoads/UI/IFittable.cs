namespace AdaptiveRoads.UI {
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IFittable {
        public void Fit2Children();
        public void Fit2Parent();
    }

    public static class IFittableExtensions {
        public static IEnumerable<IFittable> GetFitableDirectChildren(this IFittable c) {
            return (c as UIComponent)
                .GetComponentsInChildren<IFittable>()
                .Where(item => (item as UIComponent).parent == c);
        }

        public static void Fit2ParentRecursive(this IFittable c) {
            Log.Called(c);
            Log.Debug($"calling {c}.Fit2Parent() ...");
            c.Fit2Parent();
            Log.Debug($"{c}.Fit2Parent() done!");
            foreach (IFittable child in c.GetFitableDirectChildren()) {
                child.Fit2ParentRecursive();
            }
        }

        public static void Fit2ChildrenRecursive(this IFittable c) {
            Log.Called(c);Log.Flush();
            foreach (IFittable child in c.GetFitableDirectChildren()) {
                child.Fit2ChildrenRecursive();
            }
            Log.Debug($"calling {c}.Fit2Children() ...");
            c.Fit2Children();
            Log.Debug($"{c}.Fit2Children() done!");
        }

        public static void FitRecursive(this IFittable c) {
            c.Fit2ChildrenRecursive();
            c.Fit2ParentRecursive();
        }

        public static void FitRoot(this IFittable c) {
            Log.Called(c);
            if((c as UIComponent).parent is IFittable parent) {
                parent.FitRoot();
            } else {
                c.FitRecursive();
            }
        }
    }
}
