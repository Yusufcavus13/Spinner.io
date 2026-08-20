using System.Collections.Generic;
using UnityEngine;

namespace SpinForward.Core
{
    public class ParticlePool : MonoBehaviour
    {
        private static ParticlePool _instance;
        public static ParticlePool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ParticlePool>();
                    if (_instance == null)
                    {
                        var go = new GameObject("ParticlePool");
                        _instance = go.AddComponent<ParticlePool>();
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private int initialPoolSize = 30;
        
        private Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
        private Transform poolParent;

        // Shared material for all shatter particles to avoid Shader.Find allocations
        private Material shatterMat;
        public Material ShatterMaterial
        {
            get
            {
                if (shatterMat == null)
                    shatterMat = new Material(Shader.Find("Sprites/Default"));
                return shatterMat;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            poolParent = new GameObject("ParticlePool_Container").transform;
            poolParent.SetParent(transform);

            // Pre-warm the pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                pool.Enqueue(CreateNewParticle());
            }
        }

        private ParticleSystem CreateNewParticle()
        {
            GameObject psObj = new GameObject("CubeShatterEffect_Pooled");
            psObj.transform.SetParent(poolParent);
            psObj.SetActive(false);
            
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Configure base settings (ones that don't change per-shatter)
            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = 0.4f;
            main.startSpeed = 8f;
            main.startSize = 0.3f;
            main.playOnAwake = false; // Important for pooled objects
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 4, 8) });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = ShatterMaterial;

            // Add ReturnToPool script to automatically handle returning when finished
            var returnScript = psObj.AddComponent<ReturnToPool>();
            returnScript.Initialize(this);

            return ps;
        }

        public ParticleSystem GetParticle(Vector3 position, Color color)
        {
            ParticleSystem ps;
            if (pool.Count > 0)
            {
                ps = pool.Dequeue();
            }
            else
            {
                // Pool is empty, expand it
                ps = CreateNewParticle();
            }

            ps.transform.position = position;
            
            var main = ps.main;
            main.startColor = color;

            ps.gameObject.SetActive(true);
            ps.Play();

            return ps;
        }

        public void ReturnToPool(ParticleSystem ps)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }

    // Helper script to automatically return the particle system to the pool when it finishes playing
    public class ReturnToPool : MonoBehaviour
    {
        private ParticlePool pool;
        private ParticleSystem ps;

        public void Initialize(ParticlePool pool)
        {
            this.pool = pool;
            ps = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            Invoke(nameof(Return), 0.5f);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(Return));
        }

        private void Return()
        {
            if (pool != null && ps != null)
            {
                pool.ReturnToPool(ps);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
