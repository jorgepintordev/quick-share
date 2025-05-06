import { Button } from 'primereact/button';

export default function Header() {
    return (
        <>
            <span className="text-3xl font-bold">Session: </span>
            <span className="text-3xl font-bold ml-2">TE6B-4S8H</span>
            <span className="m-2" ><Button label="New" /></span>
            <span className="m-2" ><Button label="Join" /></span>
        </>
    );
}