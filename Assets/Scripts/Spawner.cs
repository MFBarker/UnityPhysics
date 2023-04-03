using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject prefab;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) 
        {
            Instantiate(prefab,this.transform.position,this.transform.rotation);
        }
    }
}
