export interface Country {
    name: string;
    code: string;
}

export interface Representative {
    name: string;
    image: string;
}

export type CustomerStatus = 'unqualified' | 'qualified' | 'new' | 'negotiation' | 'renewal' | 'proposal';

export interface Customer {
    id: number;
    name: string;
    country: Country;
    company: string;
    date: string | Date;
    status: CustomerStatus | string; // Allows other demo statuses
    verified: boolean;
    activity: number;
    representative: Representative;
    balance: number;
}

export type InventoryStatus = 'INSTOCK' | 'LOWSTOCK' | 'OUTOFSTOCK';

export interface Product {
    id: string;
    code: string;
    name: string;
    description: string;
    image: string;
    price: number;
    category: string;
    quantity: number;
    inventoryStatus: InventoryStatus | string;
    rating: number;
    orders?: ProductOrder[];
}

export interface ProductOrder {
    id?: string | number;
    productCode?: string;
    date?: string | Date;
    amount?: number;
    quantity?: number;
    status?: string;
    customer?: string;
}

export interface TreeNode {
    key: string;
    label?: string;
    data?: string;
    icon?: string;
    children?: TreeNode[];
}

export interface TreeTableNode {
    key: string;
    data: {
        name: string;
        size: string;
        type: string;
    };
    children?: TreeTableNode[];
}

export interface PhotoItem {
    itemImageSrc: string;
    thumbnailImageSrc: string;
    alt: string;
    title: string;
}
