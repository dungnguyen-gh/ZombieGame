using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
    public ResourceManager resourceManager;

    private void OnTriggerEnter(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null)
        {
            // Read the resource name and quantity from the Resource component
            string resourceName = resource.resourceName;
            int amount = resource.quantity;

            // Update the resource data
            resourceManager.CollectResource(resourceName, amount);

            //Notify the appropriate ResourceProducer about the collected resource
            resourceManager.NotifyProducer(resourceName, other.gameObject);
            // Destroy the collected resource
            //Destroy(other.gameObject);
        }
    }
}
