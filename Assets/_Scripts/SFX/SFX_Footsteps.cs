using UnityEngine;
using FMODUnity;
using UnityEngine.SceneManagement;

/// <summary>
/// Zarz¹dza odtwarzaniem dŸwiêków kroków, skoków i l¹dowania w zale¿noœci od powierzchni.
/// </summary>
public class SFX_Footsteps : MonoBehaviour
{
    public PlayerPlanetMovement ppm;

    // FMOD - Instancje zdarzeñ.
    private FMOD.Studio.EventInstance footstepsSoundInstance_hardSurface;
    private FMOD.Studio.EventInstance footstepsSoundInstance_dirtSurface;
    private FMOD.Studio.EventInstance jumpSoundInstance;
    private FMOD.Studio.EventInstance landSoundInstance;

    // Publiczne referencje do zdarzeñ FMOD.
    public EventReference footstepsEvent_dirtSurface;
    public EventReference footstepsEvent_hardSurface;
    public EventReference jumpEvent;
    public EventReference landEvent;

    // Usuniêto: private Dictionary<string, string> surfaceTags;

    private float lastFootstepTime = 0f;
    private float distToGround;
    private float footstepInterval;

    [SerializeField]
    private bool isGrounded = true;
    [SerializeField]
    private bool isJumping = false;

    void Start()
    {
        distToGround = GetComponent<Collider>().bounds.extents.y;

        // Usuniêto: Inicjalizacjê s³ownika.
    }

    void Update()
    {
        // Sprawdza, czy gracz skacze, u¿ywaj¹c spacji.
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    PlayJump();
        //}
        //Debug.Log("Horizontal Speed: " + Input.GetAxisRaw("Horizontal") + " ||| Vertical Speed: " + Input.GetAxisRaw("Vertical") + " ||| Is Grounded: " + ppm._isGrounded + " ||| Is Moving: " + isMoving);
        
    }

    void FixedUpdate()
    {

        HandleFootsteps();
    }

    /// <summary>
    /// Obs³uguje logikê odtwarzania dŸwiêków kroków.
    /// </summary>
    private void HandleFootsteps()
    {
        // Sprawdza, czy gracz siê porusza.
        bool isMoving = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);
        // Sprawdza, czy gracz biegnie.
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        if (SceneManager.GetActiveScene().name == "Test Planet Exploration")
        {
            footstepInterval = isRunning ? 0.15f : 0.45f;
        }
        else
        {
            footstepInterval = isRunning ? 0.15f : 0.44f;
        }


        if (ppm != null)
        {
            if (isMoving && ppm._isGrounded)
            {
                // Ustawia interwa³ na podstawie tego, czy gracz biegnie.

                if (Time.time - lastFootstepTime > footstepInterval)
                {
                    lastFootstepTime = Time.time;
                    PlayFootsteps();
                }
            }
        }
        else
        {
            if (isMoving && IsGrounded())
            {
               
                // Ustawia interwa³ na podstawie tego, czy gracz biegnie.
                

                if (Time.time - lastFootstepTime > footstepInterval)
                {
                    lastFootstepTime = Time.time;
                    PlayFootsteps();
                }
            }
        }
            
           
    }

    /// <summary>
    /// Odtwarza dŸwiêk kroków w zale¿noœci od powierzchni.
    /// </summary>
    private void PlayFootsteps()
    {
        if(SceneManager.GetActiveScene().name == "Test Planet Exploration")
        {
            PlaySurfaceSound(footstepsSoundInstance_dirtSurface, footstepsEvent_dirtSurface);
        }
        else
        {
            PlaySurfaceSound(footstepsSoundInstance_hardSurface, footstepsEvent_hardSurface);
        }
        
    }

    /// <summary>
    /// Odtwarza dŸwiêk skoku.
    /// </summary>
    //private void PlayJump()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        PlaySurfaceSound(jumpSoundInstance, jumpEvent);
    //    }
    //}

    /// <summary>
    /// Obs³uguje dŸwiêk l¹dowania po skoku.
    /// </summary>
    //private void OnCollisionEnter(Collision col)
    //{
    //    if (!isGrounded && isJumping)
    //    {
    //        PlayLanding();
    //    }
    //}

    /// <summary>
    /// Odtwarza dŸwiêk l¹dowania.
    /// </summary>
    //private void PlayLanding()
    //{
    //    RaycastHit hit;
    //    if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.5f))
    //    {
    //        PlaySurfaceSound(landSoundInstance, landEvent);
    //    }
    //    isGrounded = true;
    //    isJumping = false;
    //}

    /// <summary>
    /// Ogólna metoda do odtwarzania dŸwiêku na podstawie tagu powierzchni.
    /// ZASTÊPUJE S£OWNIK instrukcj¹ SWITCH.
    /// </summary>
    /// <param name="soundInstance">Instancja dŸwiêku FMOD.</param>
    /// <param name="eventRef">Referencja do zdarzenia FMOD.</param>
    /// <param name="surfaceTag">Tag powierzchni, na której znajduje siê gracz.</param>
    private void PlaySurfaceSound(FMOD.Studio.EventInstance soundInstance, EventReference eventRef)
    {
        // Zmienna przechowuj¹ca parametr FMOD. Domyœlnie ustawiona na null/pusty string.

        // Instrukcja SWITCH do mapowania Tagu na Parametr FMOD.

        // Jeœli znaleziono pasuj¹cy parametr, odtwórz dŸwiêk.
            soundInstance = RuntimeManager.CreateInstance(eventRef);
            soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));
            // Ustawia parametr FMOD na podstawie ustalonej wartoœci.
            soundInstance.start();
            soundInstance.release();
    }

    /// <summary>
    /// Sprawdza, czy gracz znajduje siê na pod³o¿u.
    /// </summary>
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, distToGround + 0.5f);
    }  
}