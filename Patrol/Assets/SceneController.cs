using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour, Observer {
    public Text scoreText;
    public Text centerText;
    
    private ScoreRecorder record;
    private UIController UI;
    private ObjectFactory fac;

    private float[] posx = { -5, 7, -5, 5 };
    private float[] posz = { -5, -7, 5, 5 };

    // Use this for initialization
    void Start () {
        record = new ScoreRecorder();
        record.scoreText = scoreText;
        UI = new UIController();
        UI.centerText = centerText;
        fac = Singleton<ObjectFactory>.Instance;


        Publish publisher = Publisher.getInstance();
        publisher.add(this);

        LoadResources();
	}
	
    private void LoadResources() {
 
        Instantiate(Resources.Load("prefabs/player"), new Vector3(5, 0, 5), Quaternion.Euler(new Vector3(0, 180, 0)));
        for (int i = 0; i < 3; i++) {
   
            GameObject patrol = fac.setObjectOnPos(new Vector3(posx[i], 0, posz[i]), Quaternion.Euler(new Vector3(0, 180, 0)));
            patrol.name = "Patrol" + (i + 1);
        }
    }
    
    /// 如果角色死亡，显示LOSE
    public void notified(ActorState state, int pos, GameObject actor) {
        if (state == ActorState.ENTER_AREA) record.addScore(1);
        else {
            UI.loseGame();
            Debug.Log("lose game");
        }
    }
}
