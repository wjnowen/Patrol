using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tem.Action;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

public class PatrolUI : SSActionManager, ISSActionCallback, Observer {

    // 各种动作
    public enum ActionState : int { IDLE, WALKLEFT, WALKFORWARD, WALKRIGHT, WALKBACK }

    // 动作
    private Animator ani;

    // 保证当前只有一个动作
    private SSAction currentAction;
    private ActionState currentState;

    // 跑步和走路的速度
    private const float walkSpeed = 1f;
    private const float runSpeed = 4f;

    // Use this for initialization
    new void Start () {
        ani = this.gameObject.GetComponent<Animator>();

        // 添加巡逻兵的事件
        Publish publisher = Publisher.getInstance();
        publisher.add(this);

        // 开始时，静止状态
        currentState = ActionState.IDLE;
        idle();
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
	}

    //回调函数，用来决定下一个动作做什么
    public void SSEventAction(SSAction source,SSActionEventType events=SSActionEventType.COMPLETED,int intParam=0,string strParam=null,Object objParam = null) {
        // 改变当前状态
        currentState = currentState > ActionState.WALKBACK ? ActionState.IDLE : (ActionState)((int)currentState + 1);

        // 执行下个动作
        switch (currentState) {
            case ActionState.WALKLEFT:
                walkLeft();
                break;
            case ActionState.WALKRIGHT:
                walkRight();
                break;
            case ActionState.WALKFORWARD:
                walkForward();
                break;
            case ActionState.WALKBACK:
                walkBack();
                break;
            default:
                idle();
                break;
        }
    }

    public void idle() {
        currentAction = IdleAction.GetIdleAction(Random.Range(1, 1.5f), ani);
        this.runAction(this.gameObject, currentAction, this);
    }

    public void walkLeft() {
        Vector3 target = Vector3.left * Random.Range(3, 5) + this.transform.position;
        currentAction = WalkAction.GetWalkAction(target, walkSpeed, ani);
        this.runAction(this.gameObject, currentAction, this);
    }

    public void walkRight() {
        Vector3 target = Vector3.right * Random.Range(3, 5) + this.transform.position;
        currentAction = WalkAction.GetWalkAction(target, walkSpeed, ani);
        this.runAction(this.gameObject, currentAction, this);
    }
    
    public void walkForward() {
        Vector3 target = Vector3.forward * Random.Range(3, 5) + this.transform.position;
        currentAction = WalkAction.GetWalkAction(target, walkSpeed, ani);
        this.runAction(this.gameObject, currentAction, this);
    }

    public void walkBack() {
        Vector3 target = Vector3.back * Random.Range(3, 5) + this.transform.position;
        currentAction = WalkAction.GetWalkAction(target, walkSpeed, ani);
        this.runAction(this.gameObject, currentAction, this);
    }

    //当碰到墙壁或者出了巡逻区域后，向反方向走
    public void turnNextDirection() {
        // 销毁当前动作
        currentAction.destory = true;

        // 往相反方向走
        switch (currentState) {
            case ActionState.WALKLEFT:
                currentState = ActionState.WALKRIGHT;
                walkRight();
                break;
            case ActionState.WALKRIGHT:
                currentState = ActionState.WALKLEFT;
                walkLeft();
                break;
            case ActionState.WALKFORWARD:
                currentState = ActionState.WALKBACK;
                walkBack();
                break;
            case ActionState.WALKBACK:
                currentState = ActionState.WALKFORWARD;
                walkForward();
                break;
        }
    }

    public void getGoal(GameObject gameobject) {
        // 销毁当前动作
        currentAction.destory = true;
        // 设置动作为跑向目标方向
        currentAction = RunAction.GetRunAction(gameobject.transform, runSpeed, ani);
        this.runAction(this.gameObject, currentAction, this);
    }

    public void loseGoal() {
        // 销毁当前动作
        currentAction.destory = true;
        // 重新进行动作循环
        idle();
    }

    public void stop() {
        // 永久站立
        currentAction.destory = true;
        currentAction = IdleAction.GetIdleAction(-1f, ani);
        this.runAction(this.gameObject, currentAction, this);
    }

    private void OnCollisionEnter(Collision collision) {
        // 撞到墙
        //Debug.Log(collision.gameObject.name);
        Transform parent = collision.gameObject.transform.parent;
        if (parent != null && parent.CompareTag("Wall")) turnNextDirection();
    }

    private void OnTriggerEnter(Collider other) {
        // 走出巡逻区域
        if (other.gameObject.CompareTag("Door")) turnNextDirection();
    }

    public void notified(ActorState state, int pos, GameObject actor) {
        if (state == ActorState.ENTER_AREA) {
            // 如果进入自己的区域，进行追击
            // 如果离开自己的区域，放弃追击
            if (pos == this.gameObject.name[this.gameObject.name.Length - 1] - '0')
                getGoal(actor);
            else loseGoal();
        }
        else stop();
        // 角色死亡，结束动作
    }
}
