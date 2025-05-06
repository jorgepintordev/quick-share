import { Button } from "primereact/button";

interface ItemProps {
    value: string;
    type?: string;  // Optional prop for type
    onRemove?: () => void;  // Optional prop for remove action
  }

export default function Item({value, type="text", onRemove}: ItemProps) {
    var icon;
    switch (type) {
        case 'text':
            icon = <i className="pi pi-pen-to-square"></i>;
            break;
        case 'file':
            icon = <i className="pi pi-file"></i>;
            break;
        case 'image':
            icon = <i className="pi pi-image"></i>;
            break;
    }

    return (
        <div className="flex items-center justify-between border-b-2 border-deep-cerulean p-2 w-full">
            <div className="w-8">
                {icon}
            </div>

            <div className="flex-1">
                <span className="text-gray-800 text-sm">{value}</span>
            </div>

            <div className="w-8">
                <Button icon="pi pi-times" rounded text severity="danger" aria-label="Delete" onClick={onRemove}/>
            </div>
        </div>
    );    
}