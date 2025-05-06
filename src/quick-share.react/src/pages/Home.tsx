// import { useState } from 'react'
import { Divider } from 'primereact/divider';
import Header from '../components/Header'
import ItemSharePanel from '../components/ItemSharePanel'
import ItemsList from '../components/ItemsList'

export default function Home() {
    // const [count, setCount] = useState(0)

  return (
    <>
        <div className="flex flex-col min-h-screen">
            {/* Header */}
            <header className="flex items-center justify-center h-20 bg-deep-cerulean text-white">
                <Header />
            </header>

            {/* Body */}
            <main className="flex flex-col items-center justify-center">
                <div className="pt-5 w-full bg-white">
                    <div className="flex flex-1 items-center justify-center">
                        <ItemSharePanel />
                    </div>
                    <Divider />
                </div>
                
                <div className="w-5/6">
                    <ItemsList />
                </div>
            </main>
        </div>
    </>
  );
}