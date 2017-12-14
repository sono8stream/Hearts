using UnityEngine;
using System.Collections;

public class InstantiateAsyncManager : MonoBehaviour
{

    public static InstantiateAsyncManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public static void InstantiateAsync(GameObject[] objects, Transform self)
    {
        Instance.StartCoroutine(Instance.InstanceObjects(objects, self));
    }

    public IEnumerator InstanceObjects(GameObject[] objects, Transform self)
    {
        self.gameObject.SetActive(false);

        foreach (var obj in objects)
        {
            var item = (GameObject)GameObject.Instantiate(obj);
            item.transform.parent = self;
            yield return null;
        }
        self.gameObject.SetActive(true);
    }
}
