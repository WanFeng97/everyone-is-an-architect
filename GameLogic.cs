using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MoveDir//enumerate all potential movement directions
{
    FrontBack,
    RightLeft,
}

public class GameLogic : MonoBehaviour
{
    public Transform topPlate;
    Transform movingPlate;

    MoveDir moveDir = MoveDir.FrontBack;
    bool inverseMove = false;

    public float speed = 1;
    float speedfactor;
    float cutfactor;

    private int scoreCount = 0;
    public Text score;

    float surfacearea = 1;
    float maxsurfacearea = 1;
    float topplatevolumn;
    float totalvolumn = 0.1f;


    bool gameOver = false;
    
    float mainSpeed = 100.0f; //regular speed
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
    float camSens = 0.25f; //How sensitive it with mouse
    private Vector3 lastMouse; //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun= 1.0f;



    void Start()
    {

    }

    void GenerateNewPlate()//Function of creating new floor plates
    {
        if (moveDir == MoveDir.FrontBack)//chnaging moving direction of floor plates
        {
            moveDir = MoveDir.RightLeft;
        }
        else
        {
            moveDir = MoveDir.FrontBack;
        }

        if (cutfactor > 1.0f)
        {
            movingPlate = Instantiate(topPlate);//Based on the top plate, get a new plate
        }
        else
        {
            movingPlate = Instantiate(topPlate);
            movingPlate.transform.localScale = new Vector3(1.5f * movingPlate.localScale.x, movingPlate.localScale.y, 1.5f * movingPlate.localScale.z);
        }

        movingPlate.name = "Plate";//Shortern the new objects' names - names becomes long if there are so many plates
        movingPlate.transform.position = new Vector3(topPlate.position.x, topPlate.position.y+0.1f, topPlate.position.z);//Change the position for the new plate
        


        MeshRenderer render = movingPlate.GetComponent<MeshRenderer>();
        Material m = render.material;
        m.color = RainbowColor(movingPlate.position.y);

        Vector3 camPos = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(camPos.x, camPos.y+0.1f, camPos.z);
    }

    Color RainbowColor(float h)
    {
        float r = Mathf.Sin(h);
        float g = Mathf.Sin(h + 120 * Mathf.Deg2Rad);
        float b = Mathf.Sin(h + 240 * Mathf.Deg2Rad);

        Color c = new Color(1-r/2, 1-g/2, 1-b/2);
        return c;
    }



    void MovePlate()
    {
        Vector3 move;
        if (moveDir == MoveDir.FrontBack)//moving front and back
        {
            move = new Vector3(0, 0, speed);
            if (inverseMove)//Set up a limited zone for movingPlate
            {
                if (movingPlate.position.z < -3 )
                {
                    inverseMove = !inverseMove;
                    GameObject atfield = Resources.Load("Prefab/AT_Field") as GameObject;
                    Instantiate(atfield, movingPlate.transform.position + new Vector3(0, 0, -0.5f * movingPlate.localScale.z), movingPlate.transform.rotation);
                }
            }
            else
            {
                if (movingPlate.position.z > 3)
                {
                    inverseMove = !inverseMove;
                    GameObject atfield = Resources.Load("Prefab/AT_Field") as GameObject;
                    Instantiate(atfield, movingPlate.transform.position + new Vector3(0, 0, 0.5f * movingPlate.localScale.z), movingPlate.transform.rotation);
                }
            }
        }
        else//moving left and right 
        {
            move = new Vector3(speed, 0, 0);
            if (inverseMove)
            {
                if (movingPlate.position.x < -3)
                {
                    inverseMove = !inverseMove;
                    GameObject atfield = Resources.Load("Prefab/AT_FieldLR") as GameObject;
                    Instantiate(atfield, movingPlate.transform.position + new Vector3(-0.5f * movingPlate.localScale.x, 0, 0), movingPlate.transform.rotation);
                }
            }
            else
            {
                if (movingPlate.position.x > 3)
                {
                    inverseMove = !inverseMove;
                    GameObject atfield = Resources.Load("Prefab/AT_FieldLR") as GameObject;
                    Instantiate(atfield, movingPlate.transform.position + new Vector3(0.5f * movingPlate.localScale.x, 0, 0), movingPlate.transform.rotation);
                }
            }
        }
        if (inverseMove)
        {
            move = -move;
        }

        movingPlate.Translate(move * Time.deltaTime);



    }

   

    void StopPlate()
    {
        CheckGameOver();
        if (gameOver)
        {
            return;
        }

        GameObject plateEffect = Resources.Load("Prefab/PlateEffect") as GameObject;
        Instantiate(plateEffect, movingPlate.transform.position + new Vector3(0, 0.06f, 0), movingPlate.transform.rotation);

        Transform stayPlate = null;
        Transform cutPlate = null;

        float movFront = movingPlate.position.z + movingPlate.localScale.z /2;
        float movBack = movingPlate.position.z - movingPlate.localScale.z /2;
        float movRight = movingPlate.position.x + movingPlate.localScale.x /2;
        float movLeft = movingPlate.position.x - movingPlate.localScale.x /2;

        float topFront = topPlate.position.z + topPlate.localScale.z /2;
        float topBack = topPlate.position.z - topPlate.localScale.z /2;
        float topRight = topPlate.position.x + topPlate.localScale.x /2;
        float topLeft = topPlate.position.x - topPlate.localScale.x /2;

        
        if (cutfactor > 1.0f && cutfactor < 3.0f)
        {
            if (moveDir == MoveDir.FrontBack)
            {
                float cutFront, cutBack, stayFront, stayBack;

                if (movingPlate.position.z > topPlate.position.z)
                {
                    cutFront = movFront;
                    cutBack = topFront;
                    stayFront = topFront;
                    stayBack = movBack;

                }
                else
                {
                    cutFront = topBack;
                    cutBack = movBack;
                    stayFront = movFront;
                    stayBack = topBack;
                }
                Destroy(movingPlate.gameObject);
                cutPlate = Instantiate(movingPlate);
                cutPlate.position = new Vector3(cutPlate.position.x, cutPlate.position.y, (cutFront + cutBack) / 2);
                cutPlate.localScale = new Vector3(cutPlate.localScale.x, cutPlate.localScale.y, cutFront - cutBack);

                stayPlate = Instantiate(movingPlate);
                stayPlate.position = new Vector3(stayPlate.position.x, stayPlate.position.y, (stayFront + stayBack) / 2);
                stayPlate.localScale = new Vector3(stayPlate.localScale.x, stayPlate.localScale.y, stayFront - stayBack);
            }
            else
            {
                float cutRight, cutLeft, stayRight, stayLeft;

                if (movingPlate.position.x > topPlate.position.x)
                {
                    cutRight = movRight;
                    cutLeft = topRight;
                    stayRight = topRight;
                    stayLeft = movLeft;

                }
                else
                {
                    cutRight = topLeft;
                    cutLeft = movLeft;
                    stayRight = movRight;
                    stayLeft = topLeft;
                }
                Destroy(movingPlate.gameObject);
                cutPlate = Instantiate(movingPlate);
                cutPlate.position = new Vector3((cutRight + cutLeft) / 2, cutPlate.position.y, cutPlate.position.z);
                cutPlate.localScale = new Vector3(cutRight - cutLeft, cutPlate.localScale.y, cutPlate.localScale.z);

                stayPlate = Instantiate(movingPlate);
                stayPlate.position = new Vector3((stayRight + stayLeft) / 2, stayPlate.position.y, stayPlate.position.z);
                stayPlate.localScale = new Vector3(stayRight - stayLeft, stayPlate.localScale.y, stayPlate.localScale.z);
            }

            cutPlate.gameObject.AddComponent<Rigidbody>();
            topPlate = stayPlate;//Otherwise movingPlate becomes new topPlate
        }
        else
        {
            topPlate = movingPlate;
        }

        

        Text pos = GameObject.Find("pos").GetComponent<Text>();
        pos.text = topPlate.position.ToString();

        Text vol = GameObject.Find("vol").GetComponent<Text>();
        vol.text = topPlate.localScale.ToString();

        Text srf = GameObject.Find("srf").GetComponent<Text>();
        srf.text = (topPlate.localScale.x * topPlate.localScale.z).ToString();

        topplatevolumn = topPlate.localScale.x * topPlate.localScale.y * topPlate.localScale.z;


        
        movingPlate = null;//There is no movingPlate now
    }

    void CheckGameOver()
    {
        float movFront = movingPlate.position.z + movingPlate.localScale.z /2;
        float movBack = movingPlate.position.z - movingPlate.localScale.z /2;
        float movRight = movingPlate.position.x + movingPlate.localScale.x /2;
        float movLeft = movingPlate.position.x - movingPlate.localScale.x /2;

        float topFront = topPlate.position.z + topPlate.localScale.z /2;
        float topBack = topPlate.position.z - topPlate.localScale.z /2;
        float topRight = topPlate.position.x + topPlate.localScale.x /2;
        float topLeft = topPlate.position.x - topPlate.localScale.x /2;


        if (moveDir == MoveDir.FrontBack)
        {
            if (movBack > topFront || movFront < topBack)//If movingPlate does not touch the topPlate
            {
                gameOver = true;//game over
                GameObject Boom = Resources.Load("Prefab/BoomEdit") as GameObject;
                Instantiate(Boom, movingPlate.transform.position + new Vector3(0, 0, 0), movingPlate.transform.rotation);

                return;
            }
        }
        else
        {
            if (movLeft > topRight || movRight < topLeft)
            {
                gameOver = true;
                GameObject Boom = Resources.Load("Prefab/BoomEdit") as GameObject;
                Instantiate(Boom, movingPlate.transform.position + new Vector3(0, 0, 0), movingPlate.transform.rotation);

                return;
            }
        }
    }
    private Vector3 GetBaseInput() 
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += new Vector3(0, 0 , 0.1f);
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity += new Vector3(0, 0, -0.1f);
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity += new Vector3(-0.1f, 0, 0);
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += new Vector3(0.1f, 0, 0);
        }
        return p_Velocity;
    }

    void Mouseangle()
    {
        lastMouse = Input.mousePosition - lastMouse ;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 );
        lastMouse = new Vector3(Camera.main.transform.eulerAngles.x + lastMouse.x , Camera.main.transform.eulerAngles.y + lastMouse.y, 0);
        Camera.main.transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;
        //Mouse  camera angle.  
    }

    void Camerachange()
    {
        float f = 0.0f;
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        { // only move while a direction key is pressed
            if (Input.GetKey (KeyCode.LeftShift))
            {
                totalRun += Time.deltaTime;
                p  = p * totalRun * shiftAdd;
                p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
                p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
                p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
            } 
            else
            {
                totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
                p = p * mainSpeed;
            }
         
            p = p * Time.deltaTime;
            Vector3 newPosition = transform.position;
            if (Input.GetKey(KeyCode.Space))
            { //If player wants to move on X and Z axis only
                Camera.main.transform.Translate(p);
                newPosition.x = transform.position.x;
                newPosition.z = transform.position.z;
                Camera.main.transform.position = newPosition;
            } 
            else 
            {
                Camera.main.transform.Translate(p);
            }
        }
    }
    IEnumerator WaitReset() 
    {
        yield return new WaitForSeconds(1f);//Wait for 1 second
        // continue process
        Mouseangle();//Change view angle
        Camerachange();//Change camera position
        
    } 

    // Update is called once per frame
    void Update()
    {
        if (gameOver)//If GameOver, then stop the game
        {
            
            Text viewyourwork = GameObject.Find("viewyourwork").GetComponent<Text>();
            viewyourwork.text = ("View Your Work: \nUse keyboard 'WASD' and mouse to control your screen, \nand find a best position to view your work! ").ToString();

            StartCoroutine(WaitReset());
            
            return;
        }
        if (movingPlate == null)//If game continues, there isn't any movingPlate, then create a new one
        {
            GenerateNewPlate();
            speedfactor = Random.Range(0.5f, 3.0f);
            speed = speedfactor;
            
            
        }

        MovePlate();

        if (Input.GetButtonDown("Jump"))//Left mouse button causes stopPlate
        {
            
            StopPlate();
            cutfactor = Random.Range(0.5f, 3.0f);

            scoreCount++;
            Text score = GameObject.Find("score").GetComponent<Text>();
            score.text = scoreCount.ToString();



            if (topPlate.localScale.x * topPlate.localScale.z > surfacearea)
            {
                maxsurfacearea = topPlate.localScale.x * topPlate.localScale.z; 
                surfacearea = maxsurfacearea;
            }

            Text lsrf = GameObject.Find("lsrf").GetComponent<Text>();
            lsrf.text = (maxsurfacearea).ToString();

            Text lvol = GameObject.Find("lvol").GetComponent<Text>();
            lvol.text = (maxsurfacearea * 0.1).ToString();

            totalvolumn += topplatevolumn;

            Text tvol = GameObject.Find("tvol").GetComponent<Text>();
            tvol.text = (totalvolumn).ToString();

        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

}
