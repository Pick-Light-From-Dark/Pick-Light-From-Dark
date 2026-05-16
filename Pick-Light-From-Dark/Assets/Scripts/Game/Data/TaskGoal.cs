using System;
using UnityEngine;

namespace Game.Data
{
    public enum TaskState
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2
    }

    [Serializable]
    public class TaskGoal
    {
        public int taskId;
        public string taskName;
        public int targetCardId;
        public int targetCount;
        public int currentCount;
        public TaskState state;
        [SerializeField] private bool isHidden = false;
        public bool IsHidden { get => isHidden; set => isHidden = value; }

        public TaskGoal() { }

        public TaskGoal(int targetCardId, int targetCount)
        {
            this.targetCardId = targetCardId;
            this.targetCount = targetCount;
            this.currentCount = 0;
            this.state = TaskState.Pending;
        }

        public bool CheckCompleted()
        {
            return currentCount >= targetCount;
        }
    }
}
