using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bala : MonoBehaviour
{

    [Tooltip("Rigidbody de la bala; arrastrar")] public Rigidbody rb; //La bala necesita un RB, la Interpolate == Interpolate y Collision Detection == Continous

    [Tooltip("GameObject del efecto de la explosion")] public GameObject explosion;    //Efecto para la explosion
    [Tooltip("layer de los enemigos a impactar")]
    public LayerMask whatIsEnemies; //Layer en la que estarian enemigos. Se seleccioan que va a recibir daño por parte de la bala o la explosion

    [Tooltip("Daño que causara la bala")]
    public int explosionDamage;     //Daño que causara la bala
    [Tooltip("Rango de la explosion causada por la bala")]
    public float explosionRange;    //Rango de la explosion
    [Tooltip("Fuerza que aplicara la explosion sobre lo que impacte")] public float explosionForce;  //Si queremos que los patos se vean afectados por la fisica de las balos y/o explosiones podemos usar esto

    [Tooltip("Cantidad de rebotes antes de desaparecer la bala")] public int maxCollisions;           //Tiempo de vida dependiendo de los rebotes
    [Tooltip("Tiempo de vida de la bala")] public float maxLifetime;           //Tiempo de vida en unidades de tiempo
    public bool explodeOnTouch = true;  //Explota al colisionar con "x"

    [Range(0f, 1f)]             //Slide en el inspector de unity
    public float bounciness;    //Rango del rebote (para el lanzagranadas)
    [Tooltip("Activar gravedad")] public bool useGravity;     //Activar si a la bala le afecta la gravedad(rebotes)

    

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

    private void Explode()  //Explosion y daño al enemigo por la misma
    {
        
        if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);

        
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);  
        for (int i = 0; i < enemies.Length; i++)
        {
            //Atencion

            //Aqui va la funcion "TakeDamage" del enemigo, en donde dice "scriptname" se reemplaza por el nombre del scrip del enemigo o el que controle el daño que recibira. Si la funcion de recibir daño es diferente, modificar el "TakeDamage" por el correspondiente. 
            ///enemies[i].GetComponent<scriptname>().TakeDamage(explosionDamage);  
            
            if (enemies[i].GetComponent<Rigidbody>())
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
        }

        //Delay para destruir 
        Invoke("Delay", 0.05f);
    }
    private void Delay()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
       
        if (collision.collider.CompareTag("Bullet")) return; //Se puede obviar esta linea si en Unity se desactiva la colison de balas con balas en los projects settings  
        collisions++;
        if (collision.collider.CompareTag("Enemy") && explodeOnTouch) Explode();
    }

    private void Setup() //Rebotes
    {
        
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        GetComponent<SphereCollider>().material = physics_mat;

       
        rb.useGravity = useGravity;   //Gravedad

    }

    
    private void OnDrawGizmosSelected()  //Para poder ver el rango en unity, modificar el color de gizmo a conveniencia 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}

