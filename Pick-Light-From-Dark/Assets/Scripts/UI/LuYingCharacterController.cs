using UnityEngine;

namespace Game.UI
{
    public class LuYingCharacterController : MonoBehaviour
    {
        [SerializeField] private float blinkIntervalMin = 2f;
        [SerializeField] private float blinkIntervalMax = 6f;
        [SerializeField] private float expressionIntervalMin = 3f;
        [SerializeField] private float expressionIntervalMax = 8f;

        private Animator animator;
        private float nextPlayTime;

        private const string StateIdle = "Idle";
        private const string StateLittleExcitedIdle = "LittleExcitedIdle";
        private const string StateExcitedIdle = "ExcitedIdle";
        private const string StateBlink = "Blink";
        private const string StateLittleExcited = "LittleExcited";
        private const string StateExcited = "Excited";
        private const string StateChew = "Chew";

        private static readonly string[] MoodIdleStates = { StateIdle, StateLittleExcitedIdle, StateExcitedIdle };
        private static readonly string[] MoodPlayStates = { StateBlink, StateLittleExcited, StateExcited };

        public int Mood
        {
            get => mood;
            set
            {
                if (mood != value)
                {
                    mood = value;
                    animator.Play(MoodIdleStates[mood], 0, 0f);
                    ScheduleNext();
                }
            }
        }
        private int mood;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            animator.Play(StateIdle, 0, 0f);
            ScheduleNext();
        }

        void Update()
        {
            // After one-shot animation finishes, return to mood idle
            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime >= 1f && !IsIdleState(state))
            {
                animator.Play(MoodIdleStates[mood], 0, 0f);
            }

            // Periodically play mood animation (blink/excited)
            if (Time.time >= nextPlayTime)
            {
                animator.Play(MoodPlayStates[mood], 0, 0f);
                ScheduleNext();
            }
        }

        bool IsIdleState(AnimatorStateInfo state)
        {
            return state.IsName(StateIdle) || state.IsName(StateLittleExcitedIdle) || state.IsName(StateExcitedIdle);
        }

        public void PlayChew()
        {
            animator.Play(StateChew, 0, 0f);
        }

        void ScheduleNext()
        {
            float min = mood == 0 ? blinkIntervalMin : expressionIntervalMin;
            float max = mood == 0 ? blinkIntervalMax : expressionIntervalMax;
            nextPlayTime = Time.time + Random.Range(min, max);
        }
    }
}
