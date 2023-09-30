using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public struct MyHud
{
    public Text txt_score;
    private int my_int_score;

    public Text txt_line;
    private int my_int_line;

    public Text txt_level;
    public int my_int_level { get;  private set; }

    public float my_speed;

    private int my_counter_line;
    public void AddScore(int _score)
    {
        my_int_score += _score;
        txt_score.text = my_int_score.ToString();
    }
    public void AddLine(int _line)
    {
        my_int_line += _line;
        txt_line.text = my_int_line.ToString();
        my_counter_line += _line;
        if (my_counter_line > 9) 
            AddLevel(1);

        my_counter_line = my_counter_line % 10;
    }
    public void AddLevel(int _level)
    {
        my_int_level += _level;
        txt_level.text = my_int_level.ToString();
        my_speed -= my_int_level > 5? 0.02f : 0.05f;
    }
}
public class my_main : MonoBehaviour
{
    private const int wid = 13, hei = 21;
    private float my_step = 1;
    private float my_curr_time;
    private GameObject pref_tetrino;
    private Object pref_tetrino_object;
    private my_tetrino_figure my_figure;
    private TetrinoFigure my_figure_random;
    private my_tetrino_element[,] my_array;
    private MyHud my_hud;
    private GameObject my_3d_camera;
    private GameObject my_main_camera;
    private GameObject my_animation_camera;
    private my_title_figure my_title;

    private AudioSource my_audio_sound;
    private AudioClip[] my_sounds;

    private void Start()
    {
        my_curr_time = 0;
        my_array = new my_tetrino_element[wid, hei];

        pref_tetrino = Resources.Load("my_prefab/my_tetrino_figure") as GameObject;
        pref_tetrino_object = Resources.Load("my_prefab/my_prefab_tetrino_o");
        my_title = FindObjectOfType < my_title_figure >();

        my_sounds = Resources.LoadAll<AudioClip>("my_sound");
        my_audio_sound = GetComponent<AudioSource>();
        MyRandomSound();

        my_main_camera = GameObject.FindGameObjectWithTag("MainCamera"); 
        my_3d_camera = GameObject.FindGameObjectWithTag("my_3d_camera");
        my_animation_camera = GameObject.FindGameObjectWithTag("my_camera_animation");
        my_animation_camera.SetActive(false);

        MyOnSwitchCamera(false);

        my_hud.txt_score = GameObject.FindGameObjectWithTag("my_score").GetComponent<Text>();
        my_hud.txt_line = GameObject.FindGameObjectWithTag("my_line").GetComponent<Text>();
        my_hud.txt_level = GameObject.FindGameObjectWithTag("my_level").GetComponent<Text>();
        my_hud.my_speed = 0.5f;
        my_hud.AddLevel(1);

        my_figure_random = CreateRandomFigure();
        CreateFigure(my_figure_random);
        my_figure_random = CreateRandomFigure();

        my_title.GetComponentInChildren<my_figure_data>().MyInitialize(my_figure_random);
        for (int y = 0; y < hei; y++)
        {
            for (int x = 0; x < wid; x++)
            {
                GameObject go = Instantiate(pref_tetrino_object, new Vector3(x * my_step, y * my_step,0), Quaternion.identity)as GameObject;
                my_array[x, y] = go.GetComponent<my_tetrino_element>();
            }
        }
    }
    private void MyRandomSound()
    {
        int index = Random.Range(0, my_sounds.Length);
        my_audio_sound.clip = my_sounds[index];
    }
    private TetrinoFigure CreateRandomFigure()
    {
        return (TetrinoFigure)Random.Range(0, 5);
    }
    private void CreateFigure(TetrinoFigure _figure)
    {
        my_figure = Instantiate(pref_tetrino, new Vector3(my_step * 6, my_step * (hei - 2)),
            Quaternion.identity).GetComponent<my_tetrino_figure>();
        my_figure.GetComponentInChildren<my_figure_data>().MyInitialize(_figure);

        if (my_hud.my_int_level<3)
            my_figure.GetComponentInChildren<my_figure_data>().MySetCOlor(Random.ColorHSV(0.4f, 0.6f, 1,1,1,1,1,1));
        else if (my_hud.my_int_level < 5)
            my_figure.GetComponentInChildren<my_figure_data>().MySetCOlor(Random.ColorHSV(0.5f, 0.7f, 1, 1, 1, 1, 1, 1));
        else if (my_hud.my_int_level < 7)
            my_figure.GetComponentInChildren<my_figure_data>().MySetCOlor(Random.ColorHSV(0.8f, 1.0f, 1, 1, 1, 1, 1, 1));
        else if (my_hud.my_int_level < 10)
            my_figure.GetComponentInChildren<my_figure_data>().MySetCOlor(Random.ColorHSV(0.2f, 0.4f, 1, 1, 1, 1, 1, 1));
        else if (my_hud.my_int_level >= 10)
            my_figure.GetComponentInChildren<my_figure_data>().MySetCOlor(Random.ColorHSV(0.3f, 0.5f, 1, 1, 1, 1, 1, 1));

        StartCoroutine(my_update(my_hud.my_speed));
    }
    private IEnumerator my_update(float _time)
    {
        while (true)
        {
            yield return new WaitForSeconds(_time);
            my_figure.MyDropTetrino(true);

            if (CheckPreIntersect(my_figure))
                break;
        }

        AddToArray();
        Destroy(my_figure.gameObject);
        MyRemoveFulline();

        if (!IsGameOver())
        {
            CreateFigure(my_figure_random);
            my_figure_random = CreateRandomFigure();
            my_title.GetComponentInChildren<my_figure_data>().MyInitialize(my_figure_random);
        }
        else 
        {
            my_main_camera.SetActive(false);
            my_3d_camera.SetActive(false);
            my_animation_camera.SetActive(true);   
            my_title.gameObject.SetActive(false);    
        }
    }
    private void AddToArray()
    {
        GameObject[] go = my_figure.GetComponentInChildren<my_figure_data>().GetTetrinoArray;
        for (int ind = 0; ind < go.Length; ind++)
        {
            int x = (int)go[ind].transform.position.x;
            int y = (int)go[ind].transform.position.y;
            my_array[x, y].set_tetrino_active(true);
            my_array[x, y].set_tetrino_active(true);
            my_array[x, y].set_color(my_figure.GetComponentInChildren<my_figure_data>().my_color);
        }
    }
    private void MyRemoveFulline()
    {
        int[] removeLine = MyCheckFullLine();
        for (int ind = 0; ind < removeLine.Length; ind++)
        {
            for (int x = 0; x < wid; x++)
                my_array[x, removeLine[ind]].set_tetrino_active(false);

            my_hud.AddScore(removeLine.Length == 4 ? 750 : 350);
        }
        if (removeLine.Length != 0)
        {
            MyRandomSound();
            my_audio_sound.Play();

            int[] empty_line = MyCheckEmptylLine();
            bool[,] arr_new_tetrino = new bool[wid, hei];
            int start_y = 0;
            my_hud.AddLine(removeLine.Length);

            for (int y = 0; y < hei; y++)
            {
                if (MySkipTheLine(empty_line, y))
                    continue;

                for (int x = 0; x < wid; x++)
                {
                    arr_new_tetrino[x, start_y] = my_array[x,y].get_isActive_tetrino();
                }
                start_y++;
            }
            MySetNewTetrinoArray(arr_new_tetrino);
        }
    }
    public void MyOnSwitchCamera(bool _is3D)
    {
        my_main_camera.SetActive(!_is3D);
        my_3d_camera.SetActive(_is3D);
    }
    public void MyOnStartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void MyOnPause(bool _isPause)
    {
        Time.timeScale = _isPause ? 0 : 1;
    }
    private bool IsGameOver()
    {
        for (int ind = 0; ind < wid; ind++)
        {
            if (my_array[ind, hei - 3].get_isActive_tetrino())
                return true;
        }
        return false;
    }
    private void MySetNewTetrinoArray(bool[,] _arr_new)
    {
        for (int y = 0; y < hei; y++)
        {
            for (int x = 0; x < wid; x++)
                my_array[x, y].set_tetrino_active(_arr_new[x, y]);
        }
    }
    private bool MySkipTheLine(int[] _empty_line, int _y)
    {
        for (int y = 0; y < _empty_line.Length; y++)
        {
            if (_empty_line[y] == _y)
                return true;
        }
        return false;
    }
    private int[] MyCheckEmptylLine()
    {
        List<int> arr = new List<int>();
        for (int ind = 0; ind < hei; ind++)
        {
            int count_line_x = 0;
            for (int x = 0; x < wid; x++)
            {
                if (my_array[x, ind].get_isActive_tetrino())
                    break;
                else
                    count_line_x++;
            }
            if (count_line_x == wid)
                arr.Add(ind);
        }
        return arr.ToArray();
    }
    private int[] MyCheckFullLine()
    {
        List<int> arr=new List<int>();
        for (int ind=0; ind < hei; ind++)
        {
            int count_line_x = 0;
            for (int x=0; x < wid; x++)
            {
                if (my_array[x, ind].get_isActive_tetrino())
                    count_line_x++;
                else
                    break;
            }
            if (count_line_x == wid)
                arr.Add(ind);
        }
        return arr.ToArray();
    }
    private void Update()
    {
        if (my_figure)
        {
            if (Input.GetButtonDown("RotateTetrino"))
            {
                my_figure.GetComponentInChildren<my_figure_data>().MyRotation(true);
                if(CheckIntersect(my_figure))
                    my_figure.GetComponentInChildren<my_figure_data>().MyRotation(false);
            }

            if (Input.GetButtonDown("LeftTetrino"))
            {
                my_curr_time = 0;
                my_figure.MySetDirection(MyDirectiomTetrino.LEFT);
                if (CheckIntersect(my_figure))
                    my_figure.MySetDirection(MyDirectiomTetrino.RIGHT);
            }

            else if (Input.GetButtonDown("RightTetrino"))
            {
                my_curr_time = 0;
                my_figure.MySetDirection(MyDirectiomTetrino.RIGHT);
                if (CheckIntersect(my_figure))
                    my_figure.MySetDirection(MyDirectiomTetrino.LEFT);
            }
            if (Input.GetButton("DownTetrino"))
                MyInputPress(MyDirectiomTetrino.DOWN, 0.04f);
            if (Input.GetButton("LeftTetrino"))
                MyInputPress(MyDirectiomTetrino.LEFT, 0.05f);
            else if (Input.GetButton("RightTetrino"))
                MyInputPress(MyDirectiomTetrino.RIGHT, 0.05f);
        
            if (Input.GetButtonUp("LeftTetrino") || Input.GetButtonUp("RightTetrino"))
            {
                my_curr_time = 0;
            }
        }
    }
    private void MyInputPress(MyDirectiomTetrino _dir, float _time)
    {
        my_curr_time += Time.deltaTime;
        if (my_curr_time > _time)
        {
            my_curr_time = 0;
            if (_dir == MyDirectiomTetrino.LEFT)
            {
                my_figure.MySetDirection(MyDirectiomTetrino.LEFT);
                if (CheckIntersect(my_figure))
                    my_figure.MySetDirection(MyDirectiomTetrino.RIGHT);
            }
            else if (_dir == MyDirectiomTetrino.RIGHT)
            {
                my_figure.MySetDirection(MyDirectiomTetrino.RIGHT);
                if (CheckIntersect(my_figure))
                    my_figure.MySetDirection(MyDirectiomTetrino.LEFT);
            }
            else if (_dir == MyDirectiomTetrino.DOWN)
            {
                my_figure.MyDropTetrino(true);
                if (CheckIntersect(my_figure))
                    my_figure.MyDropTetrino(false);
            }
        }
    }
    private bool CheckIntersect(my_tetrino_figure _figure)
    {
        for (int ind = 0; ind < _figure.GetSegments().Length; ind++)
        {
            int x = (int)_figure.GetSegments()[ind].transform.position.x;
            int y = (int)_figure.GetSegments()[ind].transform.position.y;
            bool is_intersect = IsIntersect(x, y);
            if (is_intersect)
                return is_intersect;
        }
        return false;
    }
    private bool CheckPreIntersect(my_tetrino_figure _figure)
    {
        for (int ind = 0; ind < _figure.GetSegments().Length; ind++)
        {
            int x = (int)_figure.GetSegments()[ind].transform.position.x;
            int y = (int)_figure.GetSegments()[ind].transform.position.y;
            bool is_intersect = IsIntersect(x, y);  
            if (is_intersect) 
            {
                _figure.MyDropTetrino(false);
                return is_intersect;
            }
        }
        return false;
    }
    private bool IsIntersect(int _x, int _y)
    {
        try
        {
            if (my_array[_x, _y].get_isActive_tetrino())
                return true;
        }
        catch (System.Exception ex) { return true; }
        return false;
    }
}
