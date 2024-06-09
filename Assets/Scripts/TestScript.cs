using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Navigation2D.Data;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Camera cam;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = cam.ScreenToWorldPoint(Input.mousePosition);
            StopAllCoroutines();
            StartCoroutine(_moveToPoint(Navigation2DService.GetPath(transform.position, pos, "test", "test").ToList()));
        }
    }

    IEnumerator _moveToPoint(List<Vector2> points)
    {
        while (points.Count > 0)
        {
            while (Vector3.Distance(transform.position, points[0]) > 0.01f)
            {
                transform.Translate((((Vector3) points[0]) - transform.position).normalized * Time.deltaTime * 2f);
                yield return new WaitForEndOfFrame();
            }

            points.RemoveAt(0);
        }
    }
}
