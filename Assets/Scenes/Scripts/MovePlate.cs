using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller; //controller는 게임컨트롤러와 상호작용하기 위한 게임오브젝트 필드입니다

    GameObject reference = null; //reference는 이 코드가 할당된 moveplate가 어떤 체스말의 이동경로를 나타내는지를 참조하기위한 변수입니다.

    //matrixX, matrixY는 해당 moveplate의 체스보드상의 좌표를 나타내기 위한 변수입니다.
    int matrixX;
    int matrixY;

    // attack은 해당 이동경로에 공격이 가능한 말인지를 구분하기 위한 부울 자료형입니다.
    public bool attack = false;

    public void Start() //Start()는 오브젝트가 생성되면 실행되는 메소드입니다. 만약 해당 위치가 공격 가능한 위치일 경우 moveplate의 색상을 붉게 만듭니다. 
    {
        if (attack)
        {
            // Change to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        if (attack)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);

            if (cp.name == "white_king") controller.GetComponent<Game>().Winner("black");
            if (cp.name == "black_king") controller.GetComponent<Game>().Winner("white");


            Destroy(cp);
        }

        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), reference.GetComponent<Chessman>().GetYBoard());

        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords();

        controller.GetComponent<Game>().SetPosition(reference);

        controller.GetComponent<Game>().NextTurn();

        reference.GetComponent<Chessman>().DestroyMovePlates();
    }
    //SetCoords(int x, int y)는 moveplate의 좌표를 입력하기 위한 메소드입니다. 
    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }
    //SetReference(GameObject obj)는 moveplate가 어떤 체스말의 이동경로를 나타내는지를 참조하는 reference 변수를 입력하기 위한 메소드 입니다.
    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    //위 두 메소드는 moveplate를 생성할 때 자주 사용하게 됩니다.

    //GetReference()는 다른 코드로 reference를 반환하기 위해 사용하는 메소드입니다.
    public GameObject GetReference()
    {
        return reference;
    }
}