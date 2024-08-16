using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public static List<DroppedItem> AllItems = new List<DroppedItem>();

    public Item item { get; private set; }
    public bool CanPickup => !IsPickedUp && item != null;
    public bool TriedPickupRecently { get; private set; }
    public bool IsPickedUp { get; private set; }

    public void Set(Item item)
    {
        this.item = item;
        SetColor(unblockedColor);
        UpdatePrompt();
    }

    public void SetBlockedRecently()
    {
        TriedPickupRecently = true;
        UpdatePrompt();
    }

    public void SetPickedUp(Transform target)
    {
        IsPickedUp = true;
        pickupTarget = target;
        pickupTime = Time.time;
        rb.isKinematic = true;
        SetColor(blockedColor);
        UpdatePrompt();
    }

    public void SetNearby(bool isNearby)
    {
        this.isNearby = isNearby;
        UpdatePrompt();
    }

    public void AddRandomForce(float force)
    {
        // Force upwards, random direction sideways, and random rotation
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        rb.AddForce(new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * force, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * force, ForceMode.Impulse);
    }

    [Header("Config")]
    [ColorUsage(true, true)]
    [SerializeField] private Color blockedColor;
    [ColorUsage(true, true)]
    [SerializeField] private Color unblockedColor;

    [Header("References")]
    [SerializeField] private GameObject prompt;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer meshRenderer;

    private Transform pickupTarget;
    private float pickupTime;
    private bool isNearby;

    private void Awake()
    {
        AllItems.Add(this);
        meshRenderer.material = new Material(meshRenderer.material);
    }

    private void OnDestroy()
    {
        AllItems.Remove(this);
    }

    private void Update()
    {
        // If picked up move towards target increasingly quickly, desttoy when arrived
        if (IsPickedUp)
        {
            Vector3 dir = pickupTarget.position - transform.position;

            if (dir.magnitude < 0.1f)
            {
                Destroy(gameObject);
                return;
            }

            float speed = Mathf.Lerp(0f, 10f, (Time.time - pickupTime) / 1f);
            transform.position += dir.normalized * speed * Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        prompt.transform.position = transform.position + Vector3.up * 1f;
        prompt.transform.rotation = Quaternion.identity;
    }

    private void UpdatePrompt()
    {
        prompt.SetActive(isNearby && CanPickup && TriedPickupRecently);
    }

    private void SetColor(Color color)
    {
        meshRenderer.material.color = color;
        meshRenderer.material.SetColor("_EmissionColor", color);
    }
}
