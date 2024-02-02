using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;


struct Rotaions
{
    public float CosX;
    public float SinX;
    public float CosY;
    public float SinY;

}

[Flags]
enum MovementOptions
{
    dash = 1,
    trujump = 1<<1,
    trujumpenable = 1<<2,
    onground = 1<<3,
    onwall = 1<<4,
    
    
    
    
}



public class Mov : MonoBehaviour
{
    [SerializeField] private bool enableWallrun = false;
    [SerializeField] private bool enableWallJump = false;
    [SerializeField] private bool dashEnable = false;
    [SerializeField] private bool dampYEnable = false;
    [SerializeField] private bool enableSprint = false;
    
    
    
    
   [SerializeField] private float speed;
   [SerializeField] private float camfollowervalue1 =1;
   [SerializeField] private float camfollowervalue2 =1;
   [SerializeField]private float maxspeedchange;
   [SerializeField]private float radius;
   [SerializeField]private float camspeedx;
   [SerializeField]private float camspeedy;
   [SerializeField]private float ycamoffset;
   [SerializeField]private float wallspeed;
   [SerializeField]private float walljump;
   [SerializeField]private float dashspeed;
   [SerializeField]private float dashshCooldown;
   [SerializeField]private float sprintspeed;
   [SerializeField] private float maxWallVel = 10;
   [SerializeField]private float JumpTime = 1f;
   [SerializeField] private float jumpforce = 2;
   [SerializeField] private float cayoteTime= 1;
   [SerializeField]private Transform cam;
   
   //Might put in struct later
   private MovementOptions Contorl = 0;
   private Vector3 vel = Vector3.zero;
   private Vector3 desiredvel;
   Vector3 tpos ;
   private Vector3 wallrunnorm;
   private Rigidbody movershaker;
   private Rotaions rots;
   Transform came ;
   private float xcam = 0;
   private float ycam = 1;
   private float scayoteTime= 1;
  private float jumpforce2 = 2;
  private float initalrun;
  private float walltimer = 0.4f;
  private float walltimercop = 0.4f;
  private float dashcooldownCopy = 0f;
  private const float hpi = Mathf.PI / 2;
  private float speed2;
 private float JumpTimec = -1f;
 private float plottouse =0;
 private float dotimer = 0;


 
  float jumpvel =>  Mathf.Sqrt(-2f * Physics.gravity.y * jumpforce2);
 

  // Start is called before the first frame update
    void Start()
    {
        came = cam.transform;
        Cursor.lockState = CursorLockMode.Locked ;
        scayoteTime = cayoteTime;
        movershaker = GetComponent<Rigidbody>();
        speed2 = speed;
      
        jumpforce2 = jumpforce;
        plottouse = transform.position.y;
    }


    void Update()
    {
        

        tpos = transform.position;
   
        
       
        speed2 = speed;
        
        jumpforce2 = jumpforce;
        vel = movershaker.velocity;
        
        
        //sprint
        if (Input.GetKey(KeyCode.LeftShift) && enableSprint)
        {
            speed2 *= sprintspeed;
            
            jumpforce2 *= sprintspeed;
            
        }
      //From https://catlikecoding.com/unity/tutorials/movement/physics/
         desiredvel = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical"));
        desiredvel = transform.rotation * desiredvel;
     
        xcam -= Input.GetAxis("Mouse X")*camspeedx;
        float tocheck = ycam - Input.GetAxis("Mouse Y") * camspeedy;
       
        if (tocheck <= hpi && tocheck > 0)
        {
         
            ycam = tocheck; 
            
        }

       
        desiredvel *= speed2;
   
        



       
        
    if (Input.GetKeyDown(KeyCode.Space) )
    {
        if (((Contorl & MovementOptions.onwall) == MovementOptions.onwall) && enableWallrun&&enableWallJump)
        {
            Contorl |= MovementOptions.trujump;
            Contorl |= MovementOptions.trujumpenable;
           
        }


        if (JumpTimec < 0)
        {
            JumpTimec = JumpTime;
        }




    }

    if ( dashcooldownCopy < 0 && dashEnable)
    {
        if (Input.GetMouseButtonDown(1) )
        {
            Contorl |= MovementOptions.dash;
            dashcooldownCopy = dashshCooldown;
            



        }
        
    }

        //dont have to worry about underflow https://csharp.2000things.com/tag/overflow/#:~:text=When%20you%20perform%20an%20arithmetic,the%20maximum%20allowed%20exponent%20value.
        if ((Contorl & MovementOptions.trujumpenable) != 0)
        {
            walltimercop -= Time.deltaTime;
     
        }

       
        if ((Contorl & MovementOptions.onground) == 0)
        {


            scayoteTime -= Time.deltaTime;
        }

        rots = new Rotaions()
            { CosX = Mathf.Cos(xcam), CosY = Mathf.Cos(ycam), SinX = Mathf.Sin(xcam), SinY = Mathf.Sin(ycam) };
        float time = Time.deltaTime;
        
        //to damp ys could be just the last else by itself and work just wanted more control
        //Could use hermites curve
        if (dampYEnable)
        {
            
            if (  vel.y > 0.01)
            {
                dotimer += (time*time*time)*camfollowervalue1;
                plottouse = Mathf.Lerp( plottouse,tpos.y,dotimer);
            }
            else
            {
          
         
         
                dotimer = time*camfollowervalue2 ;
                plottouse = Mathf.Lerp( plottouse,tpos.y,dotimer);
           
          
        
            }
        }

        

        
        came.position = tpos +
                        (new Vector3(rots.CosX * rots.CosY, rots.SinY,
                            rots.SinX * rots.CosY ) * radius);

        
        Vector3 tup = transform.up;
        came.rotation = Quaternion.LookRotation(tpos-came.position, tup);

        Vector3 xlook = new Vector3(rots.CosX, 0, rots.SinX);
        transform.rotation = Quaternion.LookRotation(-xlook, tup);
        Debug.DrawLine(tpos, tpos+transform.forward*2, Color.red);
       
        came.position += new Vector3(0, dampYEnable ? ycamoffset-(tpos.y-plottouse) :ycamoffset , 0);
            
       

        

      
        RaycastHit obj;
        Vector3 campos = came.position;
        if (Physics.Raycast(campos,tpos-campos,out obj,radius+0.01f))
        {
            
           if (!ReferenceEquals(obj.transform.gameObject,this.transform.gameObject) )
            {
               //Lerp the final position to  make smoother
                Vector3 ray = (tpos - campos).normalized;
                Vector3 mint  = (obj.collider.bounds.min - campos) ;
                Vector3 maxt  = (obj.collider.bounds.max - campos) ;
                mint = new Vector3(mint.x/ray.x, mint.y/ray.y, mint.z/ray.z);
                maxt = new Vector3(maxt.x/ray.x, maxt.y/ray.y, maxt.z/ray.z);

                Vector3 tocheckaa = Vector3.Max(mint, maxt);
                came.position += ray * Mathf.Min(Mathf.Min(tocheckaa.x, tocheckaa.y), tocheckaa.z);
              

            }
          
        }
       
    }

    private void OnCollisionEnter(Collision other)
    {
       //if floor the normal will point all the way up
        if (other.GetContact(0).normal.y > 0.9)
        {
            Contorl |= MovementOptions.onground;
            scayoteTime = cayoteTime;


        }
        dashcooldownCopy = -1;
        
        
      
        initalrun = Mathf.Min(movershaker.velocity.magnitude, maxWallVel);
        
    }

    private void OnCollisionExit(Collision other)
    {
      

        dashcooldownCopy = -1;

    }

    private void OnCollisionStay(Collision other)
    {
        
        if(other.GetContact(0).normal.y < 0.1 &&  other.GetContact(0).normal.y > -0.1  && enableWallrun)
        {
            
           Contorl |= MovementOptions.onwall;
            wallrunnorm = other.GetContact(0).normal;
        }

        
        dashcooldownCopy -= Time.deltaTime;
        
    }
  
    private void FixedUpdate()
    {
        //had to make a timer to disable wall running to achieve jump as it would collide in middle of jump for no reason when not touching
        if (walltimercop <= 0)
        {
            walltimercop = walltimer;
            
            Contorl &= ~MovementOptions.trujumpenable;
        }
    
       
        vel.x = Mathf.MoveTowards(vel.x, desiredvel.x, maxspeedchange * Time.deltaTime);
        vel.z = Mathf.MoveTowards(vel.z, desiredvel.z, maxspeedchange * Time.deltaTime);
        
     
       if (((Contorl & (MovementOptions.onwall | MovementOptions.trujump)) == (MovementOptions.onwall | MovementOptions.trujump) ) && enableWallrun)
        {

            //jump in opposite direction of wall plus a small jump
            vel += (wallrunnorm + new Vector3(0,0.6f,0))*walljump;
            
          Contorl &= ~MovementOptions.trujump;


        }
        
   
        else if ((Contorl & (MovementOptions.onwall | MovementOptions.trujumpenable))== (MovementOptions.onwall)&& enableWallrun)
        {
            
         
        //  Was going to convert to int16 but found out it compiles to sma code
        Vector3 dir = Vector3.Cross(cam.up, wallrunnorm)*wallspeed;
        dir = (Vector3.Dot(dir, transform.forward) > 0) ? dir : -dir;
        
           vel = dir* initalrun ;
            vel.x += desiredvel.x* Time.deltaTime;
            vel.z += desiredvel.z* Time.deltaTime;

            Debug.DrawLine(transform.position, transform.position+vel*2, Color.green);
            
        }
       
        else if (JumpTimec >0 && scayoteTime>=0)
        {
           
            vel.y += jumpvel;
            scayoteTime = -1;
            Contorl &= ~MovementOptions.onground;
            JumpTimec = 0;

        }

      
        if ((Contorl & MovementOptions.dash) !=0)
        {
           
            vel += transform.forward*dashspeed;
            Contorl &= ~MovementOptions.dash;
          
            dashcooldownCopy = 3f;
        }

        print(Contorl);
        Contorl &= ~MovementOptions.onwall;
        Debug.DrawLine(transform.position, transform.position+(wallrunnorm).normalized, Color.blue);
        movershaker.velocity = vel;
        
        JumpTimec -= Time.deltaTime;
        
    }
}
