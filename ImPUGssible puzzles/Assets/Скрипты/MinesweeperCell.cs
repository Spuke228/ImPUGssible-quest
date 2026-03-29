using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinesweeperCell : MonoBehaviour, IPointerClickHandler
{
    public bool mine;
    public int number;

    public bool opened;
    public bool flag;

    public TextMeshProUGUI text;

    Minesweeper game;

    int x;
    int y;

    Image img;

    public void Init(Minesweeper g, int px, int py)
    {
        game = g;
        x = px;
        y = py;

        mine = false;
        number = 0;
        opened = false;
        flag = false;

        img = GetComponent<Image>();

        text.text = "";
        img.color = new Color(0.8f, 0.8f, 0.8f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (opened)
                game.OpenAround(x, y);
            else
                game.OpenCell(x, y);
        }

        if (eventData.button == PointerEventData.InputButton.Right)
            ToggleFlag();
    }

    void ToggleFlag()
    {
        if (opened) return;

        flag = !flag;

        if (flag)
        {
            text.text = "🚩";
            text.color = Color.red;
            game.AddFlag();
        }
        else
        {
            text.text = "";
            game.RemoveFlag();
        }
    }

    public void Open()
    {
        if (opened) return;

        opened = true;

        img.color = Color.white;

        if (mine)
        {
            text.text = "💣";
            text.color = Color.red;
            return;
        }

        if (number > 0)
        {
            text.text = number.ToString();

            switch (number)
            {
                case 1: text.color = Color.blue; break;
                case 2: text.color = Color.green; break;
                case 3: text.color = Color.red; break;
                case 4: text.color = new Color(0, 0, 0.5f); break;
                case 5: text.color = new Color(0.5f, 0, 0); break;
                case 6: text.color = Color.cyan; break;
                case 7: text.color = Color.black; break;
                case 8: text.color = Color.gray; break;
            }
        }
    }

    public void ShowMine()
    {
        if (mine)
        {
            text.text = "💣";
            text.color = Color.red;
        }
    }
}