// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
namespace MStateMachine
{
    // Original metadata TypeDefIndex 1674; used by State.OnMessage/HandleMessage by value.
    public struct Message
    {
        public int sender;
        public int receiver;
        public MessageType msg;
        public double dispatchTime;
        public object extraInfo;
    }

    // Original metadata TypeDefIndex 1675.
    public enum MessageType
    {
        MSG_SLEEP = 0,
        MSG_WORK = 1,
        MSG_GOHOME = 2,
        MSG_GOHOMEOK = 3,
        MSG_GOOUT = 4,
        MSG_GOOUTOK = 5
    }

    // Original generic type; bodies follow its reference-shared ARM instantiation.
    public class StateMachine<Entity>
    {
        private Entity owner;
        private State<Entity> curState;
        private State<Entity> preState;
        private State<Entity> globalState;
        // RVA: 0x14B2E2C. Original calls System.Object constructor only.
        public StateMachine() : base() { }
        public State<Entity> CurState
        {
            // RVA: 0x14B2E34
            get { return curState; }
            // RVA: 0x14B2E3C
            set { curState = value; }
        }
        public State<Entity> PreState
        {
            // RVA: 0x14B2E44
            get { return preState; }
            // RVA: 0x14B2E4C
            set { preState = value; }
        }
        public State<Entity> GlobalState
        {
            // RVA: 0x14B2E54
            get { return globalState; }
            // RVA: 0x14B2E5C
            set { globalState = value; }
        }
        // RVA: 0x14B2E64. Initialization does not call Enter on any state.
        public void Init(Entity entity, State<Entity> curState, State<Entity> preState, State<Entity> globelState)
        {
            owner = entity;
            this.curState = curState;
            this.preState = preState;
            globalState = globelState;
        }
        // RVA: 0x14B2EB0. Reloads curState after the global callback, preserving reentrant changes.
        public void Update()
        {
            if (globalState != null) globalState.Execute(owner);
            if (curState != null) curState.Execute(owner);
        }
        // RVA: 0x14B2EFC. Same-state transitions also execute Exit then Enter.
        public void ChangeState(State<Entity> state)
        {
            preState = curState;
            if (curState != null) curState.Exit(owner);
            curState = state;
            if (curState != null) curState.Enter(owner);
        }
        // RVA: 0x14B2F70. This method does not invoke Exit/Enter.
        public void SetState(State<Entity> state)
        {
            preState = curState;
            curState = state;
        }
        // RVA: 0x14B2F9C. Current state gets first refusal; global state is read afterwards.
        public bool HandleMessage(Message message)
        {
            if (curState != null && curState.OnMessage(owner, message)) return true;
            if (globalState == null) return false;
            return globalState.OnMessage(owner, message);
        }
        // RVA: 0x14B307C
        public void RevertToPreState() { ChangeState(preState); }
    }
}
