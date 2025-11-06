using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : NetworkBehaviour
{
    public NetworkVariable<bool> Interacting = new NetworkVariable<bool>(false);
    
    
    [Header("Interaction Settings")]
    //This probably doesn't need to be a network variable since it will only
    // affect the prompt distance and the server does its own distance calculation on its side
    [SerializeField] private NetworkVariable<float> interactRange = new NetworkVariable<float>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float InteractionCooldown = 1;
    private float activeTime = 0;

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptprefab;
    private GameObject prompt;
    private TextMeshProUGUI interactkeyText;
    private TextMeshProUGUI promptName;
    GameObject canvas;

    [Header("Debug")]
    [SerializeField] private bool debug = true;

    private bool promptShown = false;
    private GameObject ClosestPromptable;

    private static NetworkList<NetworkObjectReference> interactablesRefs;

    void Awake()
    {
        //if (!IsOwner) return;
        EnsurePromptExists();

        //Finds all objects with the IInteract interface and caches it as their gameobject
        var interactables = FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None)
            .OfType<IInteract>()
            .Select(i => i.GetGameObject())
            .ToList();

        interactablesRefs = new NetworkList<NetworkObjectReference>();

        foreach (var netObj in interactables)
        {
            interactablesRefs.Add(new NetworkObjectReference(netObj));
        }
        if (debug)
            Debug.Log(interactables.Count());
        
    }

    //Add to the list of interactables
    public void AddInteractables(NetworkObject netobj)
    {
        //
        IInteract interactable;
        if (!netobj.TryGetComponent<IInteract>(out interactable)) return;
        interactablesRefs.Add(new NetworkObjectReference(netobj));
    }
    void EnsurePromptExists()
    {

        if (prompt == null)
        {

            canvas = GameObject.Find("Canvas");
            canvas.GetComponent<Canvas>().sortingOrder = 1;
            prompt = Instantiate(promptprefab, canvas.transform);
                
            if(debug)
                Debug.Log("Spawned local prompt UI from Resources");
            
        }
        if (prompt)
        {
            prompt.SetActive(false);

            interactkeyText = prompt.transform.Find("Interact Button").GetComponent<TextMeshProUGUI>();
            promptName = prompt.transform.Find("Name of Interactable").GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.Log("Missing \"Generic Interaction Prompt\" on the Canvas!");
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            activeTime -= Time.deltaTime;
        }

    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, interactRange.Value);
    }


    public void UpdatePromptDisplay(string interactkey)
    {
        GameObject Interactable = TryFindClosestInteractable();
        if (debug)
            Debug.Log($"Found Interactable : {(Interactable ? Interactable.name : "null")}");

        //Since there is only ever one prompt on the screen (closest object)
        //  We can keep one on the hierarchy at all times 
        if (Interactable != ClosestPromptable)
        {
            ClosestPromptable = Interactable;
            if (Interactable)
                OpenPrompt(Interactable.name, interactkey);
            else
                ClosePrompt();
        }

        //Makes the prompt follow the object
        if (promptShown && Interactable) 
            //Its boring currently with the prompt directly on the object, 
            //  could have it "float" between the object and player 
            //          (could also make the interactable show a glint or outline
            //              to emphasized that its interactive)
            prompt.transform.position = Interactable.transform.position;
            
        
    }
    private void OpenPrompt(string _name, string _interactkey)
    {
        if (!prompt) return;

        prompt.SetActive(true);
        promptName.text = _name;
        interactkeyText.text = _interactkey;
        promptShown = true;
        
    }

    private void ClosePrompt()
    {
        if (!prompt) return;
        prompt.SetActive(false);
        promptShown = false;
    }
    private GameObject TryFindClosestInteractable()
    {
        if (interactablesRefs.Count == 0) return null;
        GameObject ret = null;
        float closest = Mathf.Infinity;
        foreach (GameObject interactable in interactablesRefs)
        {

            //Skips if object is gone
            if (interactable == null) continue; 

            float distance = Vector2.Distance(transform.position, interactable.transform.position);
            if (distance < closest && distance < interactRange.Value)
            {
                ret = interactable;
                closest = distance;
            }
        }

        return ret;
    }
    [Rpc(SendTo.Server)]
    public void TryFindClosestInteractableServerRpc()
    {
        //Checks if the time passed is less than the cooldown time
        if (activeTime > 0) return;
        Debug.Log("TryFindClosestInteractableServerRpc");
        GameObject ret = null;
        float closest = Mathf.Infinity;
        foreach (GameObject interactable in interactablesRefs)
        {

            //Skips if object has been destroyed or inactive
            if (interactable == null || interactable.IsDestroyed() || !interactable.activeSelf) continue;

            float distance = Vector2.Distance(transform.position, interactable.transform.position);
            if (distance < closest && distance < interactRange.Value)
            {
                ret = interactable;
                closest = distance;

            }
        }
             
        if (ret != null) {

            //Sets current time to active value as "the last time the interaction was used"
            activeTime = InteractionCooldown;
            Interacting.Value = true;
            Debug.Log("Interacting!!!");
            ret.GetComponent<IInteract>().Interact(this.gameObject);
        }
    }
}
