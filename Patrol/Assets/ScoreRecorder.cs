using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreRecorder {

    // 计分板
    public Text scoreText;

    // 纪录分数
    private int score = -1;

    public void resetScore() {
        score = -1;
    }

    //走入新场景加分
    public void addScore(int addscore) {
        score += addscore;
        scoreText.text = "Score:" + score;
    }

    public void setDisActive() {
        scoreText.text = "";
    }

    public void setActive() {
        scoreText.text = "Score:" + score;
    }
}
