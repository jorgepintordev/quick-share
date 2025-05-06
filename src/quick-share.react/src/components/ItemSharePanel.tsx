import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";

export default function ItemSharePanel() {
    return (
        <div>
            <div className="flex flex-col gap-2">
                <label htmlFor="textItem" className="text-deep-cerulean">What do you want to share:</label>
                <div>
                    <InputText id="textItem" className="w-200" aria-describedby="textItem-help" placeholder="Type text or drop files here" />
                    <span className="ml-2"><Button icon="pi pi-plus" severity="secondary" aria-label="Add"/></span>
                </div>
                <small id="textItem-help" className="text-gray-500">
                    Maximum file size: 10MB.
                </small>
            </div>
        </div>
    );
}