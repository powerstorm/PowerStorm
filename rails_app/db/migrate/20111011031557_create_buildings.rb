class CreateBuildings < ActiveRecord::Migration
  def self.up
    create_table :buildings do |t|
      t.string :building_name
      t.integer :occupants
      t.integer :capacity
      t.integer :area

      t.timestamps
    end
  end

  def self.down
    drop_table :buildings
  end
end
