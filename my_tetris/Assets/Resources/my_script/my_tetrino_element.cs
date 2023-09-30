using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class my_tetrino_element : MonoBehaviour
{
    private my_visual_tetrino my_visual;


    private void Awake()
    {
        my_visual = GetComponentInChildren<my_visual_tetrino>();
        my_visual.gameObject.SetActive(false);

    }


    public bool get_isActive_tetrino()
    {
        return my_visual.gameObject.activeSelf;
    }

    public void set_tetrino_active(bool _isActive)
    {
        my_visual.gameObject.SetActive(_isActive);
    }

    public void set_color(Color _color)
    {
        my_visual.GetComponent<MeshRenderer>().material.color = _color;
    }

}
