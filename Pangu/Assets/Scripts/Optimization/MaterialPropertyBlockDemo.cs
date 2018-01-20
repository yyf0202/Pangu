using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MaterialPropertyBlockDemo : MonoBehaviour {
    GameObject[] listObj = null;
    GameObject[] listProp = null;

    public int objCount = 100;
    MaterialPropertyBlock prop = null;
    int colorID;

    void Start()
    {
        colorID = Shader.PropertyToID("_Color");
        prop = new MaterialPropertyBlock();
        var obj = Resources.Load("Perfabs/Sphere") as GameObject;
        listObj = new GameObject[objCount];
        listProp = new GameObject[objCount];
        for (int i = 0; i < objCount; ++i)
        {
            int x = Random.Range(-6, -2);
            int y = Random.Range(-4, 4);
            int z = Random.Range(-4, 4);
            GameObject o = Instantiate(obj);
            o.name = i.ToString();
            o.transform.localPosition = new Vector3(x, y, z);
            listObj[i] = o;
        }
        for (int i = 0; i < objCount; ++i)
        {
            int x = Random.Range(2, 6);
            int y = Random.Range(-4, 4);
            int z = Random.Range(-4, 4);
            GameObject o = Instantiate(obj);
            o.name = (objCount + i).ToString();
            o.transform.localPosition = new Vector3(x, y, z);
            listProp[i] = o;
        }
    }
	
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < objCount; ++i)
            {
                float r = Random.Range(0, 1f);
                float g = Random.Range(0, 1f);
                float b = Random.Range(0, 1f);
                listObj[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(r, g, b, 1));
            }

            sw.Stop();
            UnityEngine.Debug.Log(string.Format("material total: {0:F4} ms", (float)sw.ElapsedTicks * 1000 / Stopwatch.Frequency));
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < objCount; ++i)
            {
                float r = Random.Range(0, 1f);
                float g = Random.Range(0, 1f);
                float b = Random.Range(0, 1f);
                listProp[i].GetComponent<Renderer>().GetPropertyBlock(prop);
                prop.SetColor(colorID, new Color(r, g, b, 1));
                listProp[i].GetComponent<Renderer>().SetPropertyBlock(prop);
            }

            sw.Stop();
            UnityEngine.Debug.Log(string.Format("MaterialPropertyBlock total: {0:F4} ms", (float)sw.ElapsedTicks * 1000 / Stopwatch.Frequency));
        }
    }
}
  