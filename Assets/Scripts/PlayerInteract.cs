using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.Image;

public class PlayerInteract : NetworkBehaviour
{
    public NetworkVariable<bool> Interacting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactkey = KeyCode.E;

    //This probably doesn't need to be a network variable since it will only
    // affect the prompt distance and the server does its own distance calculation on its side
    [SerializeField] private NetworkVariable<float> interactRange = new NetworkVariable<float>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float InteractionCooldown = 1;
    
    private NetworkVariable<float> activeTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("UI Prompt")]
    [SerializeField] private GameObject prompt;
    private TextMeshProUGUI interactkeyText;
    private TextMeshProUGUI promptName;

    [Header("Debug")]
    [SerializeField] private bool debug = true;

    private bool promptShown = false;
    private GameObject ClosestPromptable;

    List<GameObject> interactables;

    void Awake()
    {
        prompt = GameObject.Find("GenericInteractPrompt");

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


        //Finds all objects with the IInteract interface and caches it as their gameobject
        interactables = FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None)
            .OfType<IInteract>()
            .Select(i => i.GetGameObject())
            .ToList();
        
        if(debug)
            Debug.Log(interactables.Count());
    }
    // Update is called once per frame
    void Update()
    {
        //InteractEvent();

    }
    public void InteractEvent()
    {
        if (IsServer && activeTime.Value >= 0)
        {
            activeTime.Value -= Time.deltaTime;
        }

        if (!IsOwner) return;


        DisplayPrompt();

        if (Input.GetKeyDown(interactkey) && ClosestPromptable && activeTime.Value <= 0)
        {
            if (debug)
            {
                Debug.Log($"{gameObject.name} pressed {interactkey}");
            }
            TryFindClosestInteractableServerRpc();
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, interactRange.Value);
    }


    private void DisplayPrompt()
    {
        GameObject Interactable = TryFindClosestInteractable();
        if (debug)
        {
            if (Interactable != null)
            {
               Debug.Log($"Found Interactable : {Interactable.name}");
            }
            else
            {
               Debug.Log($"Found Interactable : null");

            }
        }

        //Since there is only ever one prompt on the screen (closest object)
        //  We can keep one on the hierarchy at all times 
        if (Interactable != ClosestPromptable)
        {
            ClosestPromptable = Interactable;
            if (Interactable)
            {
                OpenPrompt(Interactable.name, interactkey.ToString());
            }
            else
            {
                ClosePrompt();
            }
        
        }

        //Makes the prompt follow the object
        if (promptShown) 
            prompt.transform.position = Camera.main.WorldToScreenPoint(Interactable.transform.position);
            
        
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

        GameObject ret = null;
        float closest = Mathf.Infinity;
        foreach (GameObject interactable in interactables)
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
    private void TryFindClosestInteractableServerRpc()
    {
        if (activeTime.Value > 0) return;

        GameObject ret = null;
        float closest = Mathf.Infinity;
        foreach (GameObject interactable in interactables)
        {

            //Skips if object has been destroyed or inactive
            if (interactable == null || !interactable.activeSelf) continue;

            float distance = Vector2.Distance(transform.position, interactable.transform.position);
            if (distance < closest && distance < interactRange.Value)
            {
                ret = interactable;
                closest = distance;

                activeTime.Value = InteractionCooldown;
            }
        }

        if (ret != null) { 
            Interacting.Value = true;
            ret.GetComponent<IInteract>().Interact(this.gameObject);
        }
    }
}
