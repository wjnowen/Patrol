using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tem.Action {
    public enum SSActionEventType : int { STARTED, COMPLETED }

    public interface ISSActionCallback {
        void SSEventAction(SSAction source, SSActionEventType events = SSActionEventType.COMPLETED,
            int intParam = 0, string strParam = null, Object objParam = null);
    }

    //动作基类
    public class SSAction : ScriptableObject {
        public bool enable = true;
        public bool destory = false;

        public GameObject gameObject { get; set; }
        public Transform transform { get; set; }
        public ISSActionCallback callback { get; set; }
        
        public virtual void Start() {
            throw new System.NotImplementedException("Action Start Error!");
        }

        public virtual void FixedUpdate() {
            throw new System.NotImplementedException("Physics Action Start Error!");
        }

        public virtual void Update() {
            throw new System.NotImplementedException("Action Update Error!");
        }
    }

    public class CCSequenceAction : SSAction, ISSActionCallback {
        public List<SSAction> sequence;
        public int repeat = -1;
        public int start = 0;

        public static CCSequenceAction GetSSAction(List<SSAction> _sequence,int _start = 0,int _repeat = 1) {
            CCSequenceAction actions = ScriptableObject.CreateInstance<CCSequenceAction>();
            actions.sequence = _sequence;
            actions.start = _start;
            actions.repeat = _repeat;
            return actions;
        }

        public override void Start() {
            foreach (SSAction ac in sequence) {
                ac.gameObject = this.gameObject;
                ac.transform = this.transform;
                ac.callback = this;
                ac.Start();
            }
        }

        public override void Update() {
            if (sequence.Count == 0) return;
            if (start < sequence.Count) sequence[start].Update();
        }

        public void SSEventAction(SSAction source, SSActionEventType events = SSActionEventType.COMPLETED,
            int intParam = 0, string strParam = null, Object objParam = null) //通过对callback函数的调用执行下个动作
        {
            source.destory = false;
            this.start++;
            if (this.start >= this.sequence.Count) {
                this.start = 0;
                if (this.repeat > 0) this.repeat--;
                if (this.repeat == 0) {
                    this.destory = true;
                    this.callback.SSEventAction(this);
                }
            }
        }

        private void OnDestroy() {
            this.destory = true;
        }
    }

    public class IdleAction : SSAction {
        // 站立持续时间
        private float time;
        private Animator ani;

        public static IdleAction GetIdleAction(float time,Animator ani) {
            IdleAction currentAction = ScriptableObject.CreateInstance<IdleAction>();
            currentAction.time = time;
            currentAction.ani = ani;
            return currentAction;
        }

        public override void Start() {
            // 进入站立状态
            ani.SetFloat("Speed", 0);
        }

        public override void Update() {
            // 永久站立
            if (time == -1) return;
            // 减去站立时间
            time -= Time.deltaTime;
            if (time < 0) {
                this.destory = true;
                this.callback.SSEventAction(this);
            }
        }
    }

    public class WalkAction : SSAction {

        // 移动速度和目标地点
        private float speed;
        private Vector3 target;
        private Animator ani;

        public static WalkAction GetWalkAction(Vector3 target,float speed,Animator ani) {
            WalkAction currentAction = ScriptableObject.CreateInstance<WalkAction>();
            currentAction.speed = speed;
            currentAction.target = target;
            currentAction.ani = ani;
            return currentAction;
        }

        public override void Start() {
            // 进入走路状态
            //Debug.Log("walk");
            ani.SetFloat("Speed", 0.5f);
        }

        public override void Update() {
            Quaternion rotation = Quaternion.LookRotation(target - transform.position);
            // 进行转向，转向目标方向
            if (transform.rotation != rotation) transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed * 5);
            //沿着直线方向走到目标位置
            this.transform.position = Vector3.MoveTowards(this.transform.position, target, speed * Time.deltaTime);
            if (this.transform.position == target) {
                this.destory = true;
                this.callback.SSEventAction(this);
            }
        }
    }

    public class RunAction : SSAction {
        // 移动速度和人物的transform
        private float speed;
        private Transform target;
        private Animator ani;

        public static RunAction GetRunAction(Transform target,float speed,Animator ani) {
            RunAction currentAction = ScriptableObject.CreateInstance<RunAction>();
            currentAction.speed = speed;
            currentAction.target = target;
            currentAction.ani = ani;
            return currentAction;
        }

        public override void Start() {
            // 进入跑步状态
            ani.SetFloat("Speed", 1);
        }

        public override void Update() {
            Quaternion rotation = Quaternion.LookRotation(target.position - transform.position);
            // 进行转向，转向目标方向
            if (transform.rotation != rotation) transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed * 5);

            this.transform.position = Vector3.MoveTowards(this.transform.position, target.position, speed * Time.deltaTime);
            if (Vector3.Distance(this.transform.position, target.position) < 0.5) {
                this.destory = true;
                this.callback.SSEventAction(this);
            }
        }
    }

    public class SSActionManager : MonoBehaviour {
        private Dictionary<int, SSAction> dictionary = new Dictionary<int, SSAction>();
        private List<SSAction> waitingAddAction = new List<SSAction>();
        private List<int> waitingDelete = new List<int>();

        protected void Start() {

        }

        protected void Update() {
            // 将待加入动作加入dictionary执行
            foreach (SSAction ac in waitingAddAction) dictionary[ac.GetInstanceID()] = ac;
            waitingAddAction.Clear();

            // 判断waitingList中的动作对是否执行，如果要删除，加入要删除的list，否则更新
            foreach (KeyValuePair<int,SSAction> dic in dictionary) {
                SSAction ac = dic.Value;
                if (ac.destory) waitingDelete.Add(ac.GetInstanceID());
                else if (ac.enable) ac.Update();
            }

            foreach (int id in waitingDelete) {
                SSAction ac = dictionary[id];
                dictionary.Remove(id);
                DestroyObject(ac);
            }

            // 将deletelist中的动作删除
            waitingDelete.Clear();
        }

        public void runAction(GameObject gameObject,SSAction action,ISSActionCallback callback) {
            action.gameObject = gameObject;
            action.transform = gameObject.transform;
            action.callback = callback;
            waitingAddAction.Add(action);
            action.Start();
        }
    }

    public class PYActionManager : MonoBehaviour {
        private Dictionary<int, SSAction> dictionary = new Dictionary<int, SSAction>();
        private List<SSAction> waitingAddAction = new List<SSAction>();
        private List<int> waitingDelete = new List<int>();

        protected void Start() {
            
        }

        protected void FixedUpdate() {
            // 将待加入动作加入dictionary执行
            foreach (SSAction ac in waitingAddAction) dictionary[ac.GetInstanceID()] = ac;
            waitingAddAction.Clear();

            // 如果要删除，加入要删除的list，否则更新
            foreach (KeyValuePair<int, SSAction> dic in dictionary) {
                SSAction ac = dic.Value;
                if (ac.destory) waitingDelete.Add(ac.GetInstanceID());
                else if (ac.enable) ac.FixedUpdate();
            }

            // 将deletelist中的动作删除
            foreach (int id in waitingDelete) {
                SSAction ac = dictionary[id];
                dictionary.Remove(id);
                DestroyObject(ac);
            }
            waitingDelete.Clear();
        }

        public void runAction(GameObject gameObject, SSAction action, ISSActionCallback callback) {
            action.gameObject = gameObject;
            action.transform = gameObject.transform;
            action.callback = callback;
            waitingAddAction.Add(action);
            action.Start();
        }
    }
}