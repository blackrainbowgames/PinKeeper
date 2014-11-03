using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Views
{
    public abstract class ViewBase : Script
    {
        public TweenPanel TweenPanel;
        public static ViewBase Current;

        public abstract void Open(TweenDirection direction, object meta);

        public virtual void Open(TweenDirection direction)
        {
            if (Current != null && !ReferenceEquals(Current, this))
            {
                Debug.Log(string.Format("Showing {0}, closing {1}", GetType().Name, Current == null ? null : Current.GetType().Name));
                Current.Close(direction == TweenDirection.Right ? TweenDirection.Left : TweenDirection.Right);
            }

            TaskScheduler.Kill(4000);
            TweenPanel.Hide(direction, 0);
            TweenPanel.Show(direction);
            enabled = true;
            Current = this;
        }

        protected virtual void Cleanup()
        {
        }

        private void Close(TweenDirection direction)
        {
            Cleanup();
            TweenPanel.Hide(direction);
            TaskScheduler.CreateTask(() => TweenPanel.gameObject.SetActive(false), 4000, TweenPanel.DefaultTimeout);
            enabled = false;
        }
    }
}