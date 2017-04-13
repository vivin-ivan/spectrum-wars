using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Spectrum
{
    public class Target : NetworkBehaviour
    {
        [SerializeField]
        private GameObject projectile;
        [SerializeField]
        private float projectileSpeed = 10f;

        [SyncVar(hook = "UpdateColor")]
        public int activeColorIndex;

        void UpdateColor(int hook)
        {
            Color newColor = Utilities.ColorRandomizer.colorDictionary[hook];

            this.GetComponent<MeshRenderer>().material.color = newColor;
            Debug.Log("Target's UpdateColor changed to " + hook);
        }
                
        [Server]
        public void UpdateActiveColorIndex()
        {
            int randomColorArrayLength = GameManager.Instance.RandomColorArray.Length;
            activeColorIndex = GameManager.Instance.RandomColorArray[Random.Range(0, randomColorArrayLength)];
        }

        [Server]
        public void ShootBack(Vector3 target)
        {
            /*
            Vector3 projectileVelocity = (target - this.transform.position).normalized * projectileSpeed;
            GameObject newProjectile = Instantiate(projectile, this.transform.position, Quaternion.identity);
            */
        }
    }
}