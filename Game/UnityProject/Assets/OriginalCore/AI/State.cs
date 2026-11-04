// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using MStateMachine;

// Reference-shared originals 0x14B39DC..0x14B3A44; generic declaration from metadata.
public class State<T>
{
    // RVA: 0x14B39DC. Original calls System.Object constructor only.
    public State() : base() { }
    // RVA: 0x14B39E4. Original bx lr.
    public virtual void Execute(T t) { }
    // RVA: 0x14B39E8. Original bx lr.
    public virtual void Enter(T t) { }
    // RVA: 0x14B39EC. Original bx lr.
    public virtual void Exit(T t) { }
    // RVA: 0x14B39F0. Original bx lr.
    public virtual void ReEnter(T t) { }
    // RVA: 0x14B39F4. Original mov r0,#0; bx lr, not a missing implementation.
    public virtual bool OnMessage(T t, Message message) { return false; }
    // RVA: 0x14B39FC. Original StringLiteral at metadata reference 0x1EDF1E8 is empty.
    public virtual string ToString(T t) { return ""; }
}
