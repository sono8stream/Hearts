using UnityEngine;
using System.Collections;

public class InstantiateAsyncManager : MonoBehaviour
{

    public static InstantiateAsyncManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public static void InstantiateAsync(Transform self,params GameObject[] objects)
    {
        Instance.StartCoroutine(Instance.InstanceObjects(objects, self));
    }

    IEnumerator InstanceObjects(GameObject[] objects, Transform self)
    {
        self.gameObject.SetActive(false);

        foreach (var obj in objects)
        {
            var item = Instantiate(obj);
            item.transform.parent = self;
            yield return null;
        }
        self.gameObject.SetActive(true);
    }
}
