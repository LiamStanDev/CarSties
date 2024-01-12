import { getSession } from "../actions/authAction";
import Heading from "../components/Heading";
import AuthTest from "./AuthTest";

const Session = async () => {
  const session = await getSession();

  return (
    <div>
      <Heading title="Sessoin dashboard" />
      <div className="bg-blue-200 border-2 border-blue-500">
        <h3 className="text-lg">Session Data</h3>
        <pre>{JSON.stringify(session, null, 2)}</pre>
      </div>
      <div className="mt-4">
        <AuthTest />
      </div>
    </div>
  );
};

export default Session;
