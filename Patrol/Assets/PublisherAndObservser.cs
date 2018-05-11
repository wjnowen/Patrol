using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActorState { ENTER_AREA, DEATH }

public interface Publish {
    // 发布函数
    void notify(ActorState state, int pos, GameObject actor);

    // 委托添加事件
    void add(Observer observer);

    // 委托取消事件
    void delete(Observer observer);
}

public interface Observer {
    //实现接收函数
    void notified(ActorState state, int pos, GameObject actor);
}

public class Publisher : Publish {
    private delegate void ActionUpdate(ActorState state, int pos, GameObject Actor);
    private ActionUpdate updatelist;

    private static Publisher _instance;
    public static Publisher getInstance() {
        if (_instance == null) _instance = new Publisher();
        return _instance;
    }

    public void notify(ActorState state, int pos, GameObject actor) {
        if (updatelist != null) updatelist(state, pos, actor);
    }

    public void add(Observer observer) {
        updatelist += observer.notified;
    }

    public void delete(Observer observer) {
        updatelist -= observer.notified;
    }
}