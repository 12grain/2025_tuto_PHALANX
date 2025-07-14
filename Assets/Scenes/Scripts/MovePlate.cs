using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller; //controller�� ������Ʈ�ѷ��� ��ȣ�ۿ��ϱ� ���� ���ӿ�����Ʈ �ʵ��Դϴ�

    GameObject reference = null; //reference�� �� �ڵ尡 �Ҵ�� moveplate�� � ü������ �̵���θ� ��Ÿ�������� �����ϱ����� �����Դϴ�.

    //matrixX, matrixY�� �ش� moveplate�� ü��������� ��ǥ�� ��Ÿ���� ���� �����Դϴ�.
    int matrixX;
    int matrixY;

    // attack�� �ش� �̵���ο� ������ ������ �������� �����ϱ� ���� �ο� �ڷ����Դϴ�.
    public bool attack = false;

    public void Start() //Start()�� ������Ʈ�� �����Ǹ� ����Ǵ� �޼ҵ��Դϴ�. ���� �ش� ��ġ�� ���� ������ ��ġ�� ��� moveplate�� ������ �Ӱ� ����ϴ�. 
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
    //SetCoords(int x, int y)�� moveplate�� ��ǥ�� �Է��ϱ� ���� �޼ҵ��Դϴ�. 
    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }
    //SetReference(GameObject obj)�� moveplate�� � ü������ �̵���θ� ��Ÿ�������� �����ϴ� reference ������ �Է��ϱ� ���� �޼ҵ� �Դϴ�.
    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    //�� �� �޼ҵ�� moveplate�� ������ �� ���� ����ϰ� �˴ϴ�.

    //GetReference()�� �ٸ� �ڵ�� reference�� ��ȯ�ϱ� ���� ����ϴ� �޼ҵ��Դϴ�.
    public GameObject GetReference()
    {
        return reference;
    }
}