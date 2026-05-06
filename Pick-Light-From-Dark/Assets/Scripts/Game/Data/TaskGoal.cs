using System;

namespace Game.Data
{
    /// <summary>
    /// 任务目标状态
    /// </summary>
    public enum TaskState
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2
    }

    /// <summary>
    /// 关卡任务目标定义
    /// </summary>
    [Serializable]
    public class TaskGoal
    {
        public int targetCardId;
        public int targetCount;
        public int currentCount;
        public TaskState state;

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
