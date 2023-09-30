using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MyDirectiomTetrino :int {LEFT =-1, RIGHT=1, DOWN } 


public class my_tetrino_figure : MonoBehaviour
{
    public void MyDropTetrino(bool _isPasitive)
    {
        if (_isPasitive)
            transform.Translate(0, -1, 0);
        else
            transform.Translate(0, 1, 0);
    }

    public void MySetDirection(MyDirectiomTetrino _dir)
    {
        transform.Translate((int)_dir, 0, 0);
    }


    public  my_tetrino_segment[] GetSegments()
    {
        return GetComponentsInChildren<my_tetrino_segment>();
    }

}
