export class PlanogramNote {
    id!: number;
    note!: string;
    LegacyUserId!: number | null;
    noteDate!: Date | null;
    planogramNoteNoteId!: number | null;
    noteTitle!: string | null;
    planogramId!: number;
    noteInReplyTo!: number | null;
    userId!: number;
    username!: string;
    planogramNotes!: PlanogramNote[];
    planogramNoteInReplyTo!: PlanogramNote | null;
}
