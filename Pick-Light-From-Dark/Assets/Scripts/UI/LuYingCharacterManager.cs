using UnityEngine;
using Game.Data;

namespace Game.UI
{
    public class LuYingCharacterManager : MonoBehaviour
    {
        [SerializeField] private GameObject inBedRoot;
        [SerializeField] private GameObject standRoot;

        bool lastInBed = true;

        void Awake()
        {
            if (inBedRoot == null) inBedRoot = transform.Find("InBed")?.gameObject;
            if (standRoot == null) standRoot = transform.Find("Stand")?.gameObject;
        }

        void Start()
        {
            SetInBed(true);
        }

        void Update()
        {
            var ps = PlayerState.Instance;
            if (ps != null)
            {
                bool inBed = ps.IsInBed();
                if (inBed != lastInBed)
                {
                    SetInBed(inBed);
                    lastInBed = inBed;
                }
            }
        }

        public void SetInBed(bool inBed)
        {
            if (inBedRoot != null) inBedRoot.SetActive(inBed);
            if (standRoot != null) standRoot.SetActive(!inBed);
        }

        public void SetMood(int mood)
        {
            var ctrl = GetActiveController();
            if (ctrl != null) ctrl.Mood = mood;
        }

        public void PlayChew()
        {
            var ctrl = GetActiveController();
            if (ctrl != null) ctrl.PlayChew();
        }

        public bool IsInBed => inBedRoot != null && inBedRoot.activeSelf;

        LuYingCharacterController GetActiveController()
        {
            var root = IsInBed ? inBedRoot : standRoot;
            return root?.GetComponent<LuYingCharacterController>();
        }
    }
}
