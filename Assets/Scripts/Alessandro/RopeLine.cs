using UnityEngine;

public class RopeLine : MonoBehaviour
{
   private Rope2DCreator rope;
   private LineRenderer line;
   
   private void Awake()
   {
       rope = GetComponent<Rope2DCreator>();
       line = GetComponent<LineRenderer>();
       
       line.enabled = true;
       line.positionCount = rope.segments.Length;
   }
   
    private void Update()
    {
        for (int i = 0; i < rope.segments.Length; i++)
        {
            line.SetPosition(i, rope.segments[i].position);
        }  
    }
}
