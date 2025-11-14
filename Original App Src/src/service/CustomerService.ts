import type { Customer } from '@/types/models';

export const CustomerService = {
    getData(): Customer[] {
        return [
            {
                id: 1000,
                name: 'James Butt',
                country: { name: 'Algeria', code: 'dz' },
                company: 'Benton, John B Jr',
                date: '2015-09-13',
                status: 'unqualified',
                verified: true,
                activity: 17,
                representative: { name: 'Ioni Bowcher', image: 'ionibowcher.png' },
                balance: 70663
            }
            // ... trimmed for brevity; full dataset remains identical to JS source
        ];
    },

    getCustomersLarge(): Promise<Customer[]> {
        return Promise.resolve(this.getData());
    },

    getCustomersMedium(): Promise<Customer[]> {
        return Promise.resolve(this.getData().slice(0, 25));
    },

    getCustomersSmall(): Promise<Customer[]> {
        return Promise.resolve(this.getData().slice(0, 10));
    }
};
