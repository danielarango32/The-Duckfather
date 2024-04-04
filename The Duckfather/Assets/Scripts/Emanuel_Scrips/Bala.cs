using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bala : MonoBehaviour
{

    [Tooltip("Rigidbody de la bala; arrastrar")] //La bala necesita un RB, la Interpolate == Interpolate y Collision Detection == Continous
    public Rigidbody rb; 

    [Tooltip("GameObject del efecto de la explosion")] //Efecto para la explosion
    public GameObject explosion;    

    [Tooltip("layer de los enemigos a impactar")] //Layer en la que estarian enemigos. Se seleccioan que va a recibir da�o por parte de la bala o la explosion
    public LayerMask whatIsEnemies; 

    [Tooltip("Da�o que causara la bala")]  //Da�o que causara la bala
    public int explosionDamage;    
    [Tooltip("Rango de la explosion causada por la bala")]  //Rango de la explosion
    public float explosionRange;   
    [Tooltip("Fuerza que aplicara la explosion sobre lo que impacte")] //Si queremos que los patos se vean afectados por la fisica de las balos y/o explosiones podemos usar esto
    public float explosionForce;  

    [Tooltip("Cantidad de rebotes antes de desaparecer la bala")]     //Tiempo de vida dependiendo de los rebotes
    public int maxCollisions;      
    

    [Tooltip("Tiempo de vida de la bala")] //Tiempo de vida en unidades de tiempo
    public float maxLifetime;
    
    
    public bool explodeOnTouch = true;  //Explota al colisionar con "x"



    [Range(0f, 1f)]             //Slide en el inspector de unity
    public float bounciness;    //Rango del rebote (para el lanzagranadas)


    [Tooltip("Activar gravedad")] 
    public bool useGravity;     //Activar si a la bala le afecta la gravedad(rebotes)

    

    int collisions;                     //Cantidad de rebotes que puede realizar la bala
    PhysicMaterial physics_mat;
    
    

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        if (collisions > maxCollisions) Explode();
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Explode();
        
    }

    [PunRPC]
    private void Explode()  //Explosion y da�o al enemigo por la misma
    {
        
        if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);

        
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);  
        for (int i = 0; i < enemies.Length; i++)
        {
            //Atencion

            //Aqui va la funcion "TakeDamage" del enemigo, en donde dice "scriptname" se reemplaza por el nombre del scrip del enemigo o el que controle el da�o que recibira. Si la funcion de recibir da�o es diferente, modificar el "TakeDamage" por el correspondiente. 
            ///enemies[i].GetComponent<scriptname>().TakeDamage(explosionDamage);  
            //enemies[i].GetComponent<PhotonView>().RPC("QuitarVida", RpcTarget.All, explosionDamage);
            enemies[i].GetComponent<LifeManager>().QuitarVida(explosionDamage);
            
            

            if (enemies[i].GetComponent<Rigidbody>())
            {
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
            }
                
        }
        


        //Delay para destruir 
        Invoke("Delay", 0.05f);
        Invoke("DelayExplosion", 5.0f);
    }
    private void Delay()
    {
        Destroy(gameObject);
        
    }
    private void DelayExplosion()
    {
        Destroy(explosion);
    }

    private void OnCollisionEnter(Collision collision)
    {

        Explode();
        /*
         if (collision.collider.CompareTag("Bullet")) return; //Se puede obviar esta linea si en Unity se desactiva la colison de balas con balas en los projects settings  
        collisions++;
        if (collision.collider.CompareTag("Player") && explodeOnTouch) Explode();
         
         */


    }

    private void Setup() //Rebotes
    {
        
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        GetComponent<MeshCollider>().material = physics_mat;

       
        rb.useGravity = useGravity;   //Gravedad

    }

    
    private void OnDrawGizmosSelected()  //Para poder ver el rango en unity, modificar el color de gizmo a conveniencia 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    
}

