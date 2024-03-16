using Godot;
using System;
using MainCharacter;

public class Player : MainCharacter
{
    public float movement_speed = 90.0f;
    public int hp = 80;
    public map<float playerX, float playerY> playerPosition = new map<float, playerX, float, playerY>;
    public Sprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite2D");
    }

    public override void _PhysicsProcess(float delta)
    {
        Movement();
    }

    private list<int> Movement() 
    {
        float x_mov = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        float y_mov = Input.GetActionStrength("down") - Input.GetActionStrength("up");
        Vector2 mov = new Vector2(x_mov, y_mov);
        if (mov.x > 0)
            sprite.FlipH = true;
        else if (mov.x < 0)
            sprite.FlipH = false;

        Velocity = mov.Normalized() * movement_speed;
        MoveAndSlide();
        // Noch nicht fertig. Playerposition muss befüllt werden.
        float playerX = MainCharacter.transform.position.x;
        float playerY = 
        playerPosition.add(playerX, playerY);


    }
}
    private void GetPlayerPosition
    {
        
    }


if (Playerlife < 50) 
{

}
