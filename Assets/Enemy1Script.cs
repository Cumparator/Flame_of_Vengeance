using UnityEngine;

public class Enemy1Script : MonoBehaviour
{
    public float Velocity = 10.0f;
    private Transform _player;
    
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void Update()
    {
        var dx = _player.position.x - transform.position.x;
        var dy = _player.position.y - transform.position.y;

        var move = new Vector3(dx, dy, 0);
        move.Normalize();
                
        transform.position += move * (Velocity * Time.deltaTime);
    }
}
