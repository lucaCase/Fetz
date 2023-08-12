using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DependencyInjector : MonoBehaviour
{
    private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    
    private void Awake() {
        addServices();
    }
    
    private void addServices() {
        MonoBehaviour[] injects = Find<MonoBehaviour>();

        foreach (MonoBehaviour target in injects) {
            addServices(target);
        }
    }

    public static void addServices(MonoBehaviour target) {
        Type targetType = target.GetType();
        FieldInfo[] fields = targetType.GetFields(BINDING_FLAGS);


        foreach (FieldInfo fi in fields) {
            IEnumerable<Attribute> attributes = fi.GetCustomAttributes();
            foreach (Attribute attribute in attributes) {
                if (!(attribute is DependencyAttribute)) {
                    continue;
                }

                DependencyAttribute dependencyAttribute = (DependencyAttribute) attribute;

                Component component = target.gameObject.GetComponent(fi.FieldType);
                if (component == null) {
                    component = target.gameObject.AddComponent(fi.FieldType);
                }

                fi.SetValue(target, component);
            }
        }
    }



    private T[] Find<T>() {
        return FindObjectsOfType<MonoBehaviour>().OfType<T>().ToArray();
    }
}
